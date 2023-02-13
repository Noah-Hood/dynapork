namespace Domain

open Thoth.Json.Net

module PorkBunError =
    type PBErrorResponse = { Status: string; Message: string }

    module PBErrorResponse =
        let encoder (msg: PBErrorResponse) =
            Encode.object [ "status", Encode.string msg.Status
                            "message", Encode.string msg.Message ]

        let decoder: Decoder<PBErrorResponse> =
            Decode.object (fun get ->
                { PBErrorResponse.Message = get.Required.Field "message" Decode.string
                  PBErrorResponse.Status = get.Required.Field "status" Decode.string })
