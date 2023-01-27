module RefreshTTLTests

open Expecto

open Domain
open Functions.DNSRecord

let createMockDNSRecordService (rcd: DNSRecord) : DNSRecordService = fun _ -> async { return Ok rcd }

let createFailedDNSRecordService (_) : DNSRecordService =
    fun _ -> async { return Error FailedToLoad }


[<Tests>]
let tests =
    testList
        "RefreshTTL tests"

        [

          testCase "it returns the result of the DNSRecord service correctly"
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

              let refreshCmdData =
                  { Record = dnsRcd
                    NewTTL = 1200<Second> }

              let dnsRcdSvc = createMockDNSRecordService dnsRcd

              let cmd =
                  { Data = refreshCmdData
                    TimeStamp = System.DateTime.Now }
                  |> RefreshTTL

              let expected = dnsRcd |> TTLRefreshed

              let result =
                  refreshTTL dnsRcdSvc cmd |> Async.RunSynchronously

              Expect.equal result (Ok expected) "did not return the result of the dns service correctly"

         ]
