module EditDNSRecordFunctionTests

// testing libraries
open Expecto
open JustEat.HttpClientInterception // mocking library for HttpClient
open HttpClientInterceptionUtil

let ipGenerator = Bogus.DataSets.Internet() // Bogus generator

// types necessary for test setup
open Domain.Environment
open Domain.Ping

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
              let response = { Status = "successfully changed IP Address" }

              let urlParams = DefaultCommand.URLParams

              let path = argsToPath urlParams.Domain urlParams.RecordType urlParams.Subdomain

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
              |> Async.RunSynchronously ]
