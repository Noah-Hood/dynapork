namespace Domain

open Thoth.Json.Net

module Environment =
    type APIKey = APIKey of string
    type SecretKey = SecretKey of string

    type ProgramEnvironment =
        | DEV
        | PROD

    type Credentials =
        { APIKey: APIKey
          SecretKey: SecretKey }

    type LoadProgramEnvironment = string -> ProgramEnvironment

    type LoadEnvironment = ProgramEnvironment -> Credentials

    module APIKey =
        let encoder (APIKey a) = Encode.string a

        let decoder: Decoder<APIKey> =
            Decode.index 0 Decode.string |> Decode.map APIKey

    module SecretKey =
        let encoder (SecretKey s) = Encode.string s

        let decoder: Decoder<SecretKey> =
            Decode.index 0 Decode.string
            |> Decode.map SecretKey
