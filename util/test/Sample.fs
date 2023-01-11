module Tests

open Expecto

[<Tests>]
let tests =
    testList
        "samples"
        [ testCase "hello returns the expected value"
          <| fun _ ->
              let expected = "Hello Expecto"
              Expect.equal expected expected "hello did not return the correct string" ]
