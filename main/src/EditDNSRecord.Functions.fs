namespace Functions

open System.Net.Http
open System.Text
open Thoth.Json.Net

open Domain.EditDNSRecord
open Domain.Environment
open Domain.Config

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

    let private addDomainToUrl (DomainName domain) urlStr = urlStr + "/" + domain

    let private addRecordTypeToUrl rType urlStr =
        let rString = RecordType.recordTypeToString rType

        urlStr + "/" + rString

    let private addSubdomainToUrl subdomain urlStr =
        match subdomain with
        | Some (Subdomain s) -> urlStr + "/" + s
        | None -> urlStr

    let editRecord: EditRecord =
        (fun client cmd ->
            // destructure command
            let { URLParams = urlParams } = cmd

            // destructure URL params
            let { URLParams.Domain = domain
                  URLParams.Subdomain = subdomain
                  URLParams.RecordType = recordType } =
                urlParams

            let bodyJson = EditDNSRecordCommand.encoder cmd

            let strContent =
                new StringContent(bodyJson.ToString(), Encoding.UTF8, "application/json")

            let url =
                BaseURL
                |> addDomainToUrl domain
                |> addRecordTypeToUrl recordType
                |> addSubdomainToUrl subdomain

            async {
                let! response =
                    client.PostAsync(url, strContent)
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
