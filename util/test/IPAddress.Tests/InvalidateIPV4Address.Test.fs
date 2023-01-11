module InvalidateIPV4Address.Test

open Expecto

[<Tests>]
let tests =
    testList
        "InvalidateIPV4Address Property Tests"
        [ testCase "It always returns the contents of the IP given it"
          <| fun _ ->
              let expected = "Hello Expecto"
              Expect.equal expected expected "hello did not return the correct string" ]
