module UpdatePublicIPTest

open Expecto

// Modules under test
open Domain
open Functions

[<Tests>]
let tests =
    testList
        "Update Public IP Tests"
        [ testCase "Returns None when provided with an unvalidated IP Address"
          <| fun _ ->
              let unvalidIP =
                  "192.168.1.1" |> UnvalidatedIP |> Unvalidated

              let baseState = PublicIP.Empty
              let expected: PublicIPUpdated option = None

              let actual =
                  PublicIP.updatePublicIP baseState unvalidIP

              Expect.equal actual expected "Did not return None when provided an unvalidated IP address"

          testCase "Populates the IP Address correctly from Empty"
          <| fun _ ->
              let validIP =
                  "192.168.1.1" |> ValidatedIP |> Validated

              let baseState = PublicIP.Empty

              let actual =
                  PublicIP.updatePublicIP baseState validIP

              let ipAddress = 
                actual
                |> Option.map (fun (PublicIPUpdated pipU) -> match pipU with | Empty -> )


              Expect.equal ipAddress (Some validIP) "Did not correctly update an Empty PublicIP"


          ]
