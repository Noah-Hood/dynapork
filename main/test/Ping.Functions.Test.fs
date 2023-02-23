module PingFunctionsTests

// testing libraries
open Expecto
open JustEat.HttpClientInterception // mocking library for HttpClient

// system libraries
open System.Net.Http

open Domain.Environment

// module under test
open Domain.Ping
open Functions.Ping

// Bogus generator
let ipGenerator = Bogus.DataSets.Internet()

let options =
    HttpClientInterceptorOptions()
        .ThrowsOnMissingRegistration()

let setBuilderContent
    (options: HttpClientInterceptorOptions)
    (bldr: HttpRequestInterceptionBuilder)
    (ctnt: PBPingResponse)
    =
    let strContent =
        match ctnt with
        | Success s -> s |> PBPingSuccessResponse.encoder
        | Failure f -> f |> Domain.PorkBunError.PBErrorResponse.encoder
        |> (fun x -> x.ToString())

    bldr
        .Responds()
        .WithContent(strContent)
        .RegisterWith(options)

let setBuilderExpectedContent (bldr: HttpRequestInterceptionBuilder) (ctnt: PBPingCommand) =
    let strContent =
        ctnt
        |> PBPingCommand.encoder
        |> (fun x -> x.ToString())

    bldr.ForContent(
        (fun x ->
            (x.ReadAsStringAsync()
             |> Async.AwaitTask
             |> Async.RunSynchronously) = strContent)
    )

let builderBase =
    HttpRequestInterceptionBuilder()
        .Requests()
        .ForHttps()
        .ForPost()
        .ForHost("api-ipv4.porkbun.com")
        .ForPath("api/json/v3/ping")

/// There are three possibilities:
///     1. Successful response (correct api keys, etc.) -> Ok ipAddress
///     2. Unsuccessful response (incorrect api keys) -> Error
///     3. Network failure -> Error

[<Literal>]
let ValidAPIKey = "pk1_valid_key"

[<Literal>]
let ValidSecretKey = "sk1_valid_key"

[<Tests>]
let fetchIPTests =
    testList
        "fetchIP tests"

        [

          testCase "When the API returns a successful response, returns the address correctly"
          <| fun _ ->
              // setup the command to use, and to be accepted by the builder
              let command =
                  { APIKey = APIKey ValidAPIKey
                    SecretKey = SecretKey ValidSecretKey }

              // generate a random IP Address with Bogus
              let testIP = ipGenerator.Ip()

              // set up the expected response
              let response =
                  { Status = "Success"; YourIP = testIP } |> Success

              // prime the builder to only accept the
              let bodyParseBuilder =
                  setBuilderExpectedContent builderBase command

              setBuilderContent options bodyParseBuilder response
              |> ignore

              use client = options.CreateHttpClient()

              async {
                  let! result = fetchIP client command

                  Expect.equal result (IPAddress testIP |> Ok) "did not return a success with the correct IP address"
              }
              |> Async.RunSynchronously

         ]
