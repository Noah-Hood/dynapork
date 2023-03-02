module PingFunctionsTests

// testing libraries
open Expecto
open JustEat.HttpClientInterception // mocking library for HttpClient

let ipGenerator = Bogus.DataSets.Internet() // Bogus generator

// types necessary for test setup
open Domain.Environment
open Domain.PorkBunError
open HttpClientInterceptionUtil

// module under test
open Domain.Ping
open Functions.Ping

// helper function for turning a PBPingResponse to a string
let contentToString (ctnt: PBPingResponse) =
    match ctnt with
    | Success s -> s |> PBPingSuccessResponse.encoder
    | Failure f -> f |> PBErrorResponse.encoder
    |> (fun x -> x.ToString())

// partial application for utility functions
let setBuilderContent = setBuilderContent contentToString
let setBuilderExpectedContent = setBuilderExpectedContent PBPingCommand.encoder

let createDefaultBuilder =
    fun () -> createDefaultBuilder "api-ipv4.porkbun.com" "api/json/v3/ping"

/// There are three possibilities:
///     1. Successful response (correct api keys, etc.) -> Ok ipAddress
///     2. Unsuccessful response (incorrect api keys) -> Error
///     3. Network failure -> Error

[<Literal>]
let ValidAPIKey = "pk1_valid_key"

[<Literal>]
let ValidSecretKey = "sk1_valid_key"

let builderTests setup =
    [

      test "when the API returns a successful response, returns the address correctly" {
          setup (fun builderBase options ->
              // setup the command to use, and to be accepted by the builder
              let command =
                  { APIKey = APIKey ValidAPIKey
                    SecretKey = SecretKey ValidSecretKey }

              // generate a random IP Address with Bogus
              let testIP = ipGenerator.Ip()

              // set up the expected response
              let response = { Status = "Success"; YourIP = testIP } |> Success

              // prime the builder to only accept the
              let bodyParseBuilder = setBuilderExpectedContent builderBase command

              setBuilderContent options bodyParseBuilder response
              |> ignore

              use client = options.CreateHttpClient()

              async {
                  let! result = fetchIP client command

                  Expect.equal result (IPAddress testIP |> Ok) "did not return a success with the correct IP address"
              }
              |> Async.RunSynchronously

          )
      }

      test "when the API returns a failure for an invalid API Key, returns the correct error" {
          setup (fun builderBase options ->
              let builderBase = builderBase.WithStatus(System.Net.HttpStatusCode.BadRequest)
              let expected: PBPingResult = InvalidAPIKey |> Error

              let command =
                  { APIKey = APIKey "invalidkey"
                    SecretKey = SecretKey "invalidsecretkey" }

              let response =
                  { Status = "Failure"
                    Message = "invalid api key. (002)" }
                  |> Failure

              setBuilderContent options builderBase response
              |> ignore

              use client = options.CreateHttpClient()

              async {
                  let! result = fetchIP client command

                  Expect.equal result expected "did not return an InvalidAPIKey error"
              }
              |> Async.RunSynchronously)
      }

      test "when the API returns any other error, returns a generic error with the message" {
          setup (fun builderBase options ->
              let builderBase = builderBase.WithStatus(System.Net.HttpStatusCode.BadRequest)
              let expected: PBPingResult = APIError "unspecific api error" |> Error

              let command =
                  { APIKey = APIKey ValidAPIKey
                    SecretKey = SecretKey ValidSecretKey }

              let response =
                  { Status = "Failure"
                    Message = "unspecific api error" }
                  |> Failure

              setBuilderContent options builderBase response
              |> ignore

              use client = options.CreateHttpClient()

              async {
                  let! result = fetchIP client command
                  Expect.equal result expected "did not return the message from the error"
              }
              |> Async.RunSynchronously)
      } ]

[<Tests>]
let fetchIPTests =
    builderTests (fun test ->
        let builderBase = createDefaultBuilder ()

        let options =
            HttpClientInterceptorOptions()
                .ThrowsOnMissingRegistration()

        test builderBase options)
    |> testList "fetchIPTests with Setup/Teardown"
