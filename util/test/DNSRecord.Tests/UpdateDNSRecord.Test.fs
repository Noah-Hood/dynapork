module UpdateDNSRecordtests

open Expecto

open Domain
open Functions.DNSRecord

let createMockDNSRecordService (rcd: DNSRecord) : DNSRecordService = fun _ -> async { return Ok rcd }

let createFailedDNSRecordService (_) : DNSRecordService =
    fun _ -> async { return Error FailedToLoad }

[<Tests>]
let tests =
    testList
        "UpdateDNSRecord tests"
        [ testCase "returns the result of the DNSRecord service"
          <| fun _ ->
              let data =
                  { Name = Root
                    DNSRecordType = A
                    Content =
                      (IPAddress
                          { Address = "192.168.1.254"
                            UpdatedAt = System.DateTime.Now })
                    TTL = 1200<Second> }

              let dnsRcd =
                  { Data = data
                    TS = System.DateTime.Now }

              let dnsRcdSvc = createMockDNSRecordService dnsRcd

              let cmd =
                  { Data = dnsRcd
                    TimeStamp = System.DateTime.Now }
                  |> UpdateRecord

              let expected = dnsRcd |> DNSRecordUpdated

              let result =
                  updateDNSRecord dnsRcdSvc cmd
                  |> Async.RunSynchronously

              Expect.equal result (Ok expected) "did not return the result of the dns service correctly"

          testCase "returns the error from the DNSRecord service, when the service fails"
          <| fun _ ->
              let data =
                  { Name = Root
                    DNSRecordType = A
                    Content =
                      (IPAddress
                          { Address = "192.168.1.254"
                            UpdatedAt = System.DateTime.Now })
                    TTL = 1200<Second> }

              let dnsRcd =
                  { Data = data
                    TS = System.DateTime.Now }

              let dnsRcdSvc = createFailedDNSRecordService dnsRcd

              let cmd =
                  { Data = dnsRcd
                    TimeStamp = System.DateTime.Now }
                  |> UpdateRecord

              let expected =
                  RecordNotUpdated "Failed to update DNS Record"

              let result =
                  updateDNSRecord dnsRcdSvc cmd
                  |> Async.RunSynchronously

              Expect.equal result (Error expected) "did not return the result of the dns service correctly"

          ]
