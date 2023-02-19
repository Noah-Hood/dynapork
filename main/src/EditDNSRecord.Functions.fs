namespace Functions

open System.Net.Http
open System.Text
open Thoth.Json.Net

open Domain.EditDNSRecord

module EditDNSRecord =
    [<Literal>]
    let private BaseURL =
        "https://porkbun.com/api/json/v3/dns/editByNameType"

    let private parseError (errorMsg: string) =
        match errorMsg.ToLower() with
        | "invalid domain." -> InvalidDomain
        | "invalid record id." -> InvalidRecordID
        | "edit error: we were unable to edit the dns record." -> SameContentError
        | x -> APIError x
        |> Error

    let editRecord: EditRecord =
        (fun client cmd ->
            // destructure command
            let { BodyParams = bodyParams
                  URLParams = urlParams } =
                cmd

            let { Type = recordType } = bodyParams

            // destructure URL params
            let { Domain = domain } = urlParams

            let bodyJson =
                BodyParams.encoder bodyParams

            let strContent =
                new StringContent(bodyJson.ToString(), Encoding.UTF8, "application/json")

            async {
                let! response =
                    client.PostAsync($"{BaseURL}/{domain}/{recordType}", strContent)
                    |> Async.AwaitTask

                return
                    match response.IsSuccessStatusCode with
                    | true ->
                        let resultContent =
                            response.Content.ReadAsStringAsync()
                            |> Async.AwaitTask
                            |> Async.RunSynchronously

                        let resultParsed =
                            Decode.fromString EditDNSRecordResponse.decoder (resultContent.ToString())

                        match resultParsed with
                        | Ok r -> Ok r
                        | Error e -> ResultParseError e |> Error

                    | false ->
                        let resultString =
                            response.Content.ReadAsStringAsync()
                            |> Async.AwaitTask
                            |> Async.RunSynchronously

                        let parseResult =
                            Decode.fromString Domain.PorkBunError.PBErrorResponse.decoder resultString

                        match parseResult with
                        | Ok r -> parseError r.Message
                        | Error e -> e |> ResultParseError |> Error

            })
