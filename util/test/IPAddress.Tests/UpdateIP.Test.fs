module UpdateIPTests

open Expecto

open Domain
open Functions

[<Tests>]
let tests =
    testList
        "UpdateIP tests"
        [ testCase "Returns an unchanged IP address when the address and timestamp are the same"
          <| fun _ ->
              let ipAddrStr = "192.168.1.254"
              let ts = System.DateTime.Now

              let ipAddr =
                  { Address = ipAddrStr; UpdatedAt = ts }
                  |> IPAddress

              let cmd = ipAddr |> UpdateIP

              let expected = ipAddr |> Unchanged

              let result = IPAddress.updateIP cmd ipAddr

              Expect.equal result expected "Did not return an Unchanged IP"

          testCase "Returns the same IP Address when unchanged"
          <| fun _ ->
              let ipAddrStr = "192.168.1.254"
              let ts = System.DateTime.Now

              let ipAddr =
                  { Address = ipAddrStr; UpdatedAt = ts }
                  |> IPAddress

              let cmd = ipAddr |> UpdateIP

              let result = IPAddress.updateIP cmd ipAddr

              let resultIP =
                  match result with
                  | Unchanged (IPAddress u) -> Some u.Address
                  | _ -> None

              Expect.equal resultIP (Some ipAddrStr) "Did not return the same IP address"

          testCase "Returns the same timestamp when unchanged"
          <| fun _ ->
              let ipAddrStr = "192.168.1.254"
              let ts = System.DateTime.Now

              let ipAddr =
                  { Address = ipAddrStr; UpdatedAt = ts }
                  |> IPAddress

              let cmd = ipAddr |> UpdateIP

              let result = IPAddress.updateIP cmd ipAddr

              let resultTS =
                  match result with
                  | Unchanged (IPAddress u) -> Some u.UpdatedAt
                  | _ -> None

              Expect.equal resultTS (Some ts) "Did not return the same timestamp"

          testCase "Returns the new timestamp and old IP Address when revalidated"
          <| fun _ ->
              let ipAddrStr = "192.168.1.254"
              let ts = System.DateTime.Now
              let newTS = ts + System.TimeSpan.FromMinutes(10)

              let originalIP =
                  { Address = ipAddrStr; UpdatedAt = ts }
                  |> IPAddress

              let newIP =
                  { Address = ipAddrStr
                    UpdatedAt = newTS }
                  |> IPAddress

              let cmd = newIP |> UpdateIP

              let result = IPAddress.updateIP cmd originalIP

              let result =
                  match result with
                  | Revalidated r -> Some r
                  | _ -> None

              Expect.equal result (Some newIP) "Did not return the original IP with the new timestamp"

          testCase "Returns the new timestamp and new IP Address when updated"
          <| fun _ ->
              let originalIPStr = "192.168.1.254"
              let newIPStr = "10.10.1.1"
              let ts = System.DateTime.Now
              let newTS = ts + System.TimeSpan.FromMinutes(10)

              let originalIP =
                  { Address = originalIPStr
                    UpdatedAt = ts }
                  |> IPAddress

              let newIP =
                  { Address = newIPStr
                    UpdatedAt = newTS }
                  |> IPAddress

              let cmd = newIP |> UpdateIP

              let result = IPAddress.updateIP cmd originalIP

              let result =
                  match result with
                  | Changed c -> Some c
                  | _ -> None

              Expect.equal result (Some newIP) "Did not return the new IP with the new timestamp"


          ]
