namespace Domain

open System.Net.Http

open Thoth.Json.Net
open PorkBunError

module Ping =
    type PBPingCommand =
        { SecretAPIKey: string
          APIKey: string }

    type PBPingSuccessResponse = { Status: string; YourIP: string }
    type PBPingFailureResponse = { Status: string; Message: string }

    type IPAddress = IPAddress of string

    type PBPingError =
        | InvalidAPIKey
        | JSONDecodeFailure of string
        | APIError of string
        | RequestError of string
        | GenericRequestError of string

    type PBPingResult = Result<IPAddress, PBPingError>

    type FetchIP = HttpClient -> PBPingCommand -> Async<PBPingResult>

    // Thoth coders
    module PBPingCommand =
        let encoder (cmd: PBPingCommand) =
            Encode.object [ "secretapikey", Encode.string cmd.SecretAPIKey
                            "apikey", Encode.string cmd.APIKey ]

        let decoder: Decoder<PBPingCommand> =
            Decode.object (fun get ->
                { PBPingCommand.SecretAPIKey = get.Required.Field "secretapikey" Decode.string
                  PBPingCommand.APIKey = get.Required.Field "apikey" Decode.string })

    module PBPingSuccessResponse =
        let encoder (successResponse: PBPingSuccessResponse) =
            Encode.object [ "status", Encode.string successResponse.Status
                            "yourIp", Encode.string successResponse.YourIP ]

        let decoder: Decoder<PBPingSuccessResponse> =
            Decode.object (fun get ->
                { PBPingSuccessResponse.Status = get.Required.Field "status" Decode.string
                  PBPingSuccessResponse.YourIP = get.Required.Field "yourIp" Decode.string })

    module PBPingFailureResponse =
        let encoder = PBErrorResponse.encoder
        let decoder = PBErrorResponse.decoder
