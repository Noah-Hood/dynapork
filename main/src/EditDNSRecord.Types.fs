namespace Domain

open System.Net.Http
open Thoth.Json.Net

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
        { SecretAPIKey: string
          APIKey: string
          Name: string option
          Type: RecordType
          Content: string
          TTL: int option
          Prio: string option }

    type URLParams =
        { Domain: string
          Subdomain: string option }

    type EditDNSRecordCommand =
        { BodyParams: BodyParams
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
        let encoder (rt: RecordType) = 
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
            |> Encode.string

        let decoder : Decoder<RecordType> = 
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
                | Error e -> Decode.fail $"Invalid RecordType received: {e}"
                )


    module BodyParams =
        let encoder (cmd: BodyParams) =
            Encode.object [ "secretapikey", Encode.string cmd.SecretAPIKey
                            "apikey", Encode.string cmd.APIKey
                            "name", Encode.option Encode.string cmd.Name
                            "type", RecordType.encoder cmd.Type
                            "content", Encode.string cmd.Content
                            "ttl", Encode.option Encode.int cmd.TTL
                            "prio", Encode.option Encode.string cmd.Prio ]

        let decoder: Decoder<BodyParams> =
            Decode.object (fun get ->
                { BodyParams.SecretAPIKey = get.Required.Field "secretapikey" Decode.string
                  BodyParams.APIKey = get.Required.Field "apikey" Decode.string
                  BodyParams.Name = get.Optional.Field "name" Decode.string
                  BodyParams.Type = get.Required.Field "type" RecordType.decoder
                  BodyParams.Content = get.Required.Field "content" Decode.string
                  BodyParams.TTL = get.Optional.Field "ttl" Decode.int
                  BodyParams.Prio = get.Optional.Field "prio" Decode.string })

    module EditDNSRecordResponse =
        let encoder (rsp: EditDNSRecordResponse) =
            Encode.object [ "status", Encode.string rsp.Status ]

        let decoder: Decoder<EditDNSRecordResponse> =
            Decode.object (fun get -> { EditDNSRecordResponse.Status = get.Required.Field "status" Decode.string })
