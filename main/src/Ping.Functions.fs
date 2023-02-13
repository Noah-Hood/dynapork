namespace Functions

open System.Net.Http
open System.Text

open Thoth.Json.Net
open Domain.Ping

module Ping =
    [<Literal>]
    let PingURL =
        "https://api-ipv4.porkbun.com/api/json/v3/ping"

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

            let cmdStrContent =
                new StringContent(cmdJSON.ToString(), Encoding.UTF8, "application/json")

            async {
                let! result =
                    client.PostAsync(PingURL, cmdStrContent)
                    |> Async.AwaitTask

                let! content =
                    result.Content.ReadAsStringAsync()
                    |> Async.AwaitTask

                let decodeResult =
                    Decode.fromString PBPingResponse.decoder content

                return
                    match decodeResult with
                    | Ok r ->
                        match r with
                        | PBPingSuccess s -> (IPAddress s.YourIP) |> Ok // successfully retrieved, decoded; valid result
                        | PBPingFailure _ -> InvalidAPIKey |> Error // successfully retrieved, decoded; invalid result
                    | Error e -> (JSONDecodeFailure e) |> Error // successfully retrieved, unsuccessfully decoded
            }
