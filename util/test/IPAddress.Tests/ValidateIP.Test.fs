module ValidateIPTests

open Expecto

open Domain
open Functions.IPAddress

[<Tests>]
let tests =
    testList
        "ValidateIPTests"
        [ testCase "It returns a validated IP address for a valid IP"
          <| fun _ ->
              let validIPStr = "192.168.1.1"

              let expected =
                  validIPStr |> ValidatedIP |> Validated |> Ok

              let actual = validateIP (UnvalidatedIP validIPStr)

              Expect.equal actual expected "hello did not return the correct string"

          testCase "It should return an InvalidQuartets error for a malformed ipAddress"
          <| fun _ ->
              let invalidIpStr = "1111."

              let expected = InvalidQuartets |> Error

              let actual = validateIP (UnvalidatedIP invalidIpStr)
              Expect.equal actual expected "Did not return an error for a malformed IP address"


          ]
