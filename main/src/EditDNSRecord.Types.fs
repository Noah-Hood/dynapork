namespace Domain

open System.Net.Http
open Thoth.Json.Net

module EditDNSRecord =
    type EditDNSRecordBodyParams =
        { SecretAPIKey: string
          APIKey: string
          Name: string option
          Type: string
          Content: string
          TTL: int option
          Prio: string option }

    type EditDNSRecordURLParams =
        { Domain: string
          Subdomain: string option }

    type EditDNSRecordCommand =
        { BodyParams: EditDNSRecordBodyParams
          URLParams: EditDNSRecordURLParams }

    type EditDNSRecordResponse = { Status: string }

    type EditDNSRecordError =
        | APIError of string
        | InvalidDomain
        | InvalidRecordID
        | ResultParseError of string

    type EditDNSRecordResult = Result<EditDNSRecordResponse, EditDNSRecordError>

    type EditRecord = HttpClient -> EditDNSRecordCommand -> Async<EditDNSRecordResult>

    module EditDNSRecordBodyParams =
        let encoder (cmd: EditDNSRecordBodyParams) =
            Encode.object [ "secretapikey", Encode.string cmd.SecretAPIKey
                            "apikey", Encode.string cmd.APIKey
                            "name", Encode.option Encode.string cmd.Name
                            "type", Encode.string cmd.Type
                            "content", Encode.string cmd.Content
                            "ttl", Encode.option Encode.int cmd.TTL
                            "prio", Encode.option Encode.string cmd.Prio ]

        let decoder: Decoder<EditDNSRecordBodyParams> =
            Decode.object (fun get ->
                { EditDNSRecordBodyParams.SecretAPIKey = get.Required.Field "secretapikey" Decode.string
                  EditDNSRecordBodyParams.APIKey = get.Required.Field "apikey" Decode.string
                  EditDNSRecordBodyParams.Name = get.Optional.Field "name" Decode.string
                  EditDNSRecordBodyParams.Type = get.Required.Field "type" Decode.string
                  EditDNSRecordBodyParams.Content = get.Required.Field "content" Decode.string
                  EditDNSRecordBodyParams.TTL = get.Optional.Field "ttl" Decode.int
                  EditDNSRecordBodyParams.Prio = get.Optional.Field "prio" Decode.string })

    module EditDNSRecordResponse =
        let encoder (rsp: EditDNSRecordResponse) =
            Encode.object [ "status", Encode.string rsp.Status ]

        let decoder: Decoder<EditDNSRecordResponse> =
            Decode.object (fun get -> { EditDNSRecordResponse.Status = get.Required.Field "status" Decode.string })
