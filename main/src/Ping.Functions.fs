namespace Functions

open System.Net.Http
open System.Text
open Thoth.Json.Net

open Domain.Ping
open Functions.Common

module Ping =
    [<Literal>]
    let private PingURL =
        "https://api-ipv4.porkbun.com/api/json/v3/ping"

    let private parseErrorMessage (msg: string) =
        match msg.ToLower() with
        | "invalid api key. (002)" -> InvalidAPIKey
        | _ -> msg |> APIError
        |> Error

    /// <summary>
    /// Fetches the public IP address of the client by pinging the PorkBun
    /// Ping API Endpoint
    /// </summary>
    /// <param name="client">The HTTPClient with which to hit the API</param>
    /// <param name="cmd">
    /// The PBPingCommand object containing the
    /// 'secret' api key and the regular apikey from the PorkBun website</param>
    let fetchIP: FetchIP =
        fun client cmd ->
            let cmdJSON = PBPingCommand.encoder cmd

            let cmdStrContent = jsonToStringContent cmdJSON

            async {
                try
                    let! result =
                        client.PostAsync(PingURL, cmdStrContent)
                        |> Async.AwaitTask

                    let! contentString = result.Content |> httpContentToString

                    return
                        match result.IsSuccessStatusCode with
                        | true ->
                            let successValue =
                                Decode.fromString PBPingSuccessResponse.decoder contentString

                            match successValue with
                            | Ok s -> s.YourIP |> IPAddress |> Ok
                            | Error e -> e |> JSONDecodeFailure |> Error
                        | false ->
                            let failureValue =
                                Decode.fromString PBPingFailureResponse.decoder contentString

                            match failureValue with
                            | Ok s -> parseErrorMessage s.Message
                            | Error e -> e |> JSONDecodeFailure |> Error
                with
                | :? HttpRequestException as e ->
                    return
                        $"Failed to fetch IP; check internet connection: {e.Message}"
                        |> RequestError
                        |> Error
                | _ as e ->
                    return
                        $"Failed to fetch IP: {e.Message}"
                        |> GenericRequestError
                        |> Error
            }
