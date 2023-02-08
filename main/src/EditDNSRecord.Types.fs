namespace DNSRecord

open Thoth.Json.Net

module EditDNSRecord =
    type EditDNSRecordCommand =
        { SecretAPIKey: string
          APIKey: string
          Name: string option
          Type: string
          Content: string
          TTL: int option
          Prio: string option }

    type EditDNSRecordResponse = { Status: string }

    module EditDNSRecordCommand =
        let encoder (cmd: EditDNSRecordCommand) =
            Encode.object [ "secretapikey", Encode.string cmd.SecretAPIKey
                            "apikey", Encode.string cmd.APIKey
                            "name", Encode.option Encode.string cmd.Name
                            "type", Encode.string cmd.Type
                            "content", Encode.string cmd.Content
                            "ttl", Encode.option Encode.int cmd.TTL
                            "prio", Encode.option Encode.string cmd.Prio ]

        let decoder: Decoder<EditDNSRecordCommand> =
            Decode.object (fun get ->
                { EditDNSRecordCommand.SecretAPIKey = get.Required.Field "secretapikey" Decode.string
                  EditDNSRecordCommand.APIKey = get.Required.Field "apikey" Decode.string
                  EditDNSRecordCommand.Name = get.Optional.Field "name" Decode.string
                  EditDNSRecordCommand.Type = get.Required.Field "type" Decode.string
                  EditDNSRecordCommand.Content = get.Required.Field "content" Decode.string
                  EditDNSRecordCommand.TTL = get.Optional.Field "ttl" Decode.int
                  EditDNSRecordCommand.Prio = get.Optional.Field "prio" Decode.string })

    module EditDNSRecordResponse =
        let encoder (rsp: EditDNSRecordResponse) =
            Encode.object [ "status", Encode.string rsp.Status ]

        let decoder: Decoder<EditDNSRecordResponse> =
            Decode.object (fun get -> { EditDNSRecordResponse.Status = get.Required.Field "status" Decode.string })
