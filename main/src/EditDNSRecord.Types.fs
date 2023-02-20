namespace Domain

open System.Net.Http
open Thoth.Json.Net

open Domain.Environment
open Domain.Ping

module EditDNSRecord =
    type RecordType =
        | A
        | MX
        | CNAME
        | ALIAS
        | TXT
        | NS
        | AAAA
        | SRV
        | TLSA
        | CAA

    type BodyParams =
        { Content: IPAddress
          TTL: int option
          Prio: string option }

    type URLParams =
        { Domain: DomainName
          Subdomain: Subdomain option
          RecordType: RecordType }

    type EditDNSRecordCommand =
        { Credentials: Credentials
          BodyParams: BodyParams
          URLParams: URLParams }

    type EditDNSRecordResponse = { Status: string }

    type EditDNSRecordError =
        | APIError of string
        | InvalidDomain
        | InvalidRecordID
        | SameContentError
        | ResultParseError of string

    type EditDNSRecordResult = Result<EditDNSRecordResponse, EditDNSRecordError>

    type EditRecord = HttpClient -> EditDNSRecordCommand -> Async<EditDNSRecordResult>

    module RecordType =
        let recordTypeToString rt =
            match rt with
            | A -> "A"
            | MX -> "MX"
            | CNAME -> "CNAME"
            | ALIAS -> "ALIAS"
            | TXT -> "TXT"
            | NS -> "NS"
            | AAAA -> "AAAA"
            | SRV -> "SRV"
            | TLSA -> "TLSA"
            | CAA -> "CAA"

        let encoder (rt: RecordType) =
            rt |> recordTypeToString |> Encode.string

        let decoder: Decoder<RecordType> =
            Decode.index 0 Decode.string
            |> Decode.andThen (fun rts ->
                let res =
                    match rts with
                    | "A" -> Ok A
                    | "MX" -> Ok MX
                    | "CNAME" -> Ok CNAME
                    | "ALIAS" -> Ok ALIAS
                    | "TXT" -> Ok TXT
                    | "NS" -> Ok NS
                    | "AAAA" -> Ok AAAA
                    | "SRV" -> Ok SRV
                    | "TLSA" -> Ok TLSA
                    | "CAA" -> Ok CAA
                    | x -> Error x // if not one of these, disallowed

                match res with
                | Ok a -> a |> Decode.succeed
                | Error e -> Decode.fail $"Invalid RecordType received: {e}")


    module BodyParams =
        let encoder (cmd: BodyParams) =
            let { Content = IPAddress content } = cmd

            Encode.object [ "content", Encode.string content
                            "ttl", Encode.option Encode.int cmd.TTL
                            "prio", Encode.option Encode.string cmd.Prio ]

        let decoder: Decoder<BodyParams> =
            Decode.object (fun get ->
                { BodyParams.Content =
                    get.Required.Field "content" Decode.string
                    |> IPAddress
                  BodyParams.TTL = get.Optional.Field "ttl" Decode.int
                  BodyParams.Prio = get.Optional.Field "prio" Decode.string })

    module EditDNSRecordCommand =
        let encoder (cmd: EditDNSRecordCommand) =
            let { Credentials = credentials
                  BodyParams = bodyParams } =
                cmd

            let { APIKey = apiKey
                  SecretKey = secretKey } =
                credentials

            let { Content = content
                  TTL = ttl
                  Prio = prio } =
                bodyParams

            Encode.object [ "content", IPAddress.encoder content
                            "ttl", Encode.option Encode.int ttl
                            "prio", Encode.option Encode.string prio
                            "apikey", APIKey.encoder apiKey
                            "secretapikey", SecretKey.encoder secretKey ]

    module EditDNSRecordResponse =
        let encoder (rsp: EditDNSRecordResponse) =
            Encode.object [ "status", Encode.string rsp.Status ]

        let decoder: Decoder<EditDNSRecordResponse> =
            Decode.object (fun get -> { EditDNSRecordResponse.Status = get.Required.Field "status" Decode.string })
