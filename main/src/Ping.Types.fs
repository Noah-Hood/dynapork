namespace Domain

open System.Net.Http

open Thoth.Json.Net
open PorkBunError
open Domain.Environment

module Ping =
    type PBPingCommand = Credentials

    type PBPingSuccessResponse = { Status: string; YourIP: string }

    type PBPingResponse =
        | Success of PBPingSuccessResponse
        | Failure of PBErrorResponse

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
            let { APIKey = (APIKey apiKey)
                  SecretKey = (SecretKey secret) } =
                cmd

            Encode.object [ "secretapikey", Encode.string secret
                            "apikey", Encode.string apiKey ]

        let decoder: Decoder<PBPingCommand> =
            Decode.object (fun get ->
                { PBPingCommand.SecretKey =
                    get.Required.Field "secretapikey" Decode.string
                    |> SecretKey
                  PBPingCommand.APIKey =
                    get.Required.Field "apikey" Decode.string
                    |> APIKey })

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

    module IPAddress =
        let encoder (IPAddress a) = Encode.string a

        let decoder: Decoder<IPAddress> =
            Decode.index 0 Decode.string
            |> Decode.map IPAddress
