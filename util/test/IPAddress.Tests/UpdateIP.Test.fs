module UpdateIPTests

open Expecto

open Domain
open Functions.IPAddress

[<Tests>]
let tests =
    testList
        "UpdateIP tests"
        [ testCase "Returns the original IP address unchanged when presented with a matching IP"
          <| fun _ ->
              let validIP = "192.168.1.1" |> IPAddress

              let expected = validIP |> Unchanged

              let command = validIP |> UpdateIP
              let result = updateIP command validIP

              Expect.equal result expected "Did not leave the original IP address unchanged"

          testCase "Returns an updated IP address when presented with a new IP Address"
          <| fun _ ->
              let validIP = "192.168.1.1" |> IPAddress
              let newValidIP = "10.10.10.1" |> IPAddress

              let command = newValidIP |> UpdateIP

              let expected = newValidIP |> Changed

              let result = updateIP command validIP

              Expect.equal result expected "Did not return a changed IP when given a new IP"

          ]
