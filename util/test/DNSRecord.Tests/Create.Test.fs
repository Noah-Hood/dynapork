module CreateDNSRecordTests

open Expecto

// module under test
open Domain

[<Tests>]
let tests =
    testList
        "DNSRecord.Create tests"
        [

          testCase "successfully creates a DNSRecord when given good input"
          <| fun _ ->
              let ipAddr =
                  { Address = "192.168.1.254"
                    UpdatedAt = System.DateTime.Now }
                  |> IPAddress

              let dnsData =
                  { Name = Root
                    DNSRecordType = A
                    Content = ipAddr
                    TTL = 1200<Second> }

              let result =
                  match DNSRecord.create dnsData with
                  | Error _ -> None
                  | Ok r -> Some r.Data

              Expect.equal result (Some dnsData) "did not correctly create a DNSRecord when given good input"

          testCase "returns an EmptyCustomName error when given a blank name"
          <| fun _ ->
              let ipAddr =
                  { Address = "192.168.1.254"
                    UpdatedAt = System.DateTime.Now }
                  |> IPAddress

              let dnsData =
                  { Name = (Custom "")
                    DNSRecordType = A
                    Content = ipAddr
                    TTL = 1200<Second> }

              let expected =
                  EmptyCustomNameError |> EmptyCustomName |> Error

              let result = DNSRecord.create dnsData

              Expect.equal result expected "did not return the correct error when given invalid input"

          testCase "returns an InvalidTTL error when given a TTL that is below the Porkbun minimum"
          <| fun _ ->
              let ipAddr =
                  { Address = "192.168.1.254"
                    UpdatedAt = System.DateTime.Now }
                  |> IPAddress

              let dnsData =
                  { Name = (Custom "Custom")
                    DNSRecordType = TXT
                    Content = ipAddr
                    TTL = 200<Second> }

              let expected = InvalidTTLError |> InvalidTTL |> Error

              let result = DNSRecord.create dnsData

              Expect.equal result expected "did not return the correct error when given invalid input"

          ]
