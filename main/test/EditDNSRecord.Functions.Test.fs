module EditDNSRecordFunctionTests

// testing libraries
open Expecto
open JustEat.HttpClientInterception // mocking library for HttpClient
open HttpClientInterceptionUtil

let ipGenerator = Bogus.DataSets.Internet() // Bogus generator

// types necessary for test setup
open Domain.Config
open Domain.Environment
open Domain.Ping
open Domain.PorkBunError

// module under test
open Domain.EditDNSRecord
open Functions.EditDNSRecord

[<Literal>]
let BaseURL = "porkbun.com"

[<Literal>]
let BasePath = "api/json/v3/dns/editByNameType"

let DefaultCredentials =
    { APIKey = APIKey "pk1_valid"
      SecretKey = SecretKey "sk1_valid" }

let DefaultBodyParams =
    { Content = IPAddress "192.168.1.1"
      TTL = None
      Prio = None }

let DefaultURLParams =
    { Domain = DomainName "website.ext"
      Subdomain = None
      RecordType = A }

let DefaultCommand =
    { Credentials = DefaultCredentials
      BodyParams = DefaultBodyParams
      URLParams = DefaultURLParams }

let editResponseToStr (rsp: EditDNSRecordResponse) =
    rsp
    |> EditDNSRecordResponse.encoder
    |> (fun s -> s.ToString())

let argsToPath (DomainName dn) (rtype: RecordType) (sd: Option<Subdomain>) =
    let rtString = RecordType.recordTypeToString rtype

    BasePath
    |> (fun bp -> bp + "/" + dn + "/" + rtString)
    |> (fun dnrt ->
        match sd with
        | Some (Subdomain s) -> dnrt + "/" + s
        | None -> dnrt)

[<Tests>]
let editDNSRecordTests =
    testList
        "EditDNSRecordTests"
        [

          testCase "returns the status message correctly"
          <| fun _ ->
              let response =
                  { Status = "successfully changed IP Address" }

              let urlParams = DefaultCommand.URLParams

              let path =
                  argsToPath urlParams.Domain urlParams.RecordType urlParams.Subdomain

              let builder =
                  (createDefaultBuilder "porkbun.com" path)
                      .WithContent(editResponseToStr response)

              let options =
                  HttpClientInterceptorOptions()
                      .ThrowsOnMissingRegistration()

              let client = options.CreateHttpClient()

              builder.RegisterWith(options) |> ignore

              async {
                  let! result = editRecord client DefaultCommand

                  Expect.equal
                      result
                      (Ok { Status = "successfully changed IP Address" })
                      "did not return the status message properly"
              }
              |> Async.RunSynchronously

          testCase "returns a ResultParseError for an unexpected result"
          <| fun _ ->
              let response = """{"staytus": "success"}"""

              let urlParams = DefaultCommand.URLParams

              let path =
                  argsToPath urlParams.Domain urlParams.RecordType urlParams.Subdomain

              let builder =
                  (createDefaultBuilder "porkbun.com" path)
                      .WithContent(response)

              let options =
                  HttpClientInterceptorOptions()
                      .ThrowsOnMissingRegistration()

              let client = options.CreateHttpClient()

              builder.RegisterWith(options) |> ignore

              async {
                  let! result = editRecord client DefaultCommand

                  let isCorrectError =
                      match result with
                      | Error (ResultParseError _) -> true
                      | _ -> false

                  Expect.isTrue isCorrectError "did not return a ResultParseError"

              }
              |> Async.RunSynchronously


          testCase "returns an invalidDomain error when given an invalid domain response"
          <| fun _ ->
              let response: PBErrorResponse =
                  { Status = "Error"
                    Message = "invalid domain." }

              let urlParams = DefaultCommand.URLParams

              let path =
                  argsToPath urlParams.Domain urlParams.RecordType urlParams.Subdomain

              let builder =
                  (createDefaultBuilder "porkbun.com" path)
                      .WithStatus(System.Net.HttpStatusCode.BadRequest)
                      .WithContent((PBErrorResponse.encoder response).ToString())

              let options =
                  HttpClientInterceptorOptions()
                      .ThrowsOnMissingRegistration()

              let client = options.CreateHttpClient()

              builder.RegisterWith(options) |> ignore

              async {
                  let! result = editRecord client DefaultCommand

                  let isCorrectError =
                      match result with
                      | Error (InvalidDomain) -> true
                      | _ -> false

                  Expect.isTrue isCorrectError "did not return an InvalidDomain error"

              }
              |> Async.RunSynchronously

          testCase "returns an invalidRecordID error when given an invalid record id response"
          <| fun _ ->
              let response: PBErrorResponse =
                  { Status = "Error"
                    Message = "invalid record id." }

              let urlParams = DefaultCommand.URLParams

              let path =
                  argsToPath urlParams.Domain urlParams.RecordType urlParams.Subdomain

              let builder =
                  (createDefaultBuilder "porkbun.com" path)
                      .WithStatus(System.Net.HttpStatusCode.BadRequest)
                      .WithContent((PBErrorResponse.encoder response).ToString())

              let options =
                  HttpClientInterceptorOptions()
                      .ThrowsOnMissingRegistration()

              let client = options.CreateHttpClient()

              builder.RegisterWith(options) |> ignore

              async {
                  let! result = editRecord client DefaultCommand

                  let isCorrectError =
                      match result with
                      | Error InvalidRecordID -> true
                      | _ -> false

                  Expect.isTrue isCorrectError "did not return an InvalidRecordID error"

              }
              |> Async.RunSynchronously

          testCase "returns a sameContent error when given a response indicating inability to edit record"
          <| fun _ ->
              let response: PBErrorResponse =
                  { Status = "Error"
                    Message = "edit error: we were unable to edit the dns record." }

              let urlParams = DefaultCommand.URLParams

              let path =
                  argsToPath urlParams.Domain urlParams.RecordType urlParams.Subdomain

              let builder =
                  (createDefaultBuilder "porkbun.com" path)
                      .WithStatus(System.Net.HttpStatusCode.BadRequest)
                      .WithContent((PBErrorResponse.encoder response).ToString())

              let options =
                  HttpClientInterceptorOptions()
                      .ThrowsOnMissingRegistration()

              let client = options.CreateHttpClient()

              builder.RegisterWith(options) |> ignore

              async {
                  let! result = editRecord client DefaultCommand

                  let isCorrectError =
                      match result with
                      | Error SameContentError -> true
                      | _ -> false

                  Expect.isTrue isCorrectError "did not return a SameContentError"

              }
              |> Async.RunSynchronously

          testCase "returns an APIError when given a response other than one quantified"
          <| fun _ ->
              let response: PBErrorResponse =
                  { Status = "Error"
                    Message = "Unspecified error message" }

              let urlParams = DefaultCommand.URLParams

              let path =
                  argsToPath urlParams.Domain urlParams.RecordType urlParams.Subdomain

              let builder =
                  (createDefaultBuilder "porkbun.com" path)
                      .WithStatus(System.Net.HttpStatusCode.BadRequest)
                      .WithContent((PBErrorResponse.encoder response).ToString())

              let options =
                  HttpClientInterceptorOptions()
                      .ThrowsOnMissingRegistration()

              let client = options.CreateHttpClient()

              builder.RegisterWith(options) |> ignore

              async {
                  let! result = editRecord client DefaultCommand

                  Expect.equal
                      result
                      (Error(APIError "unspecified error message"))
                      "did not return the error message for an unspecified API error"

              }
              |> Async.RunSynchronously

          ]
