module Tests

open Expecto
open Util.Say

[<Tests>]
let tests =
  testList "samples" [
    testCase "hello returns the expected value" <| fun _ ->
      let expected = "Hello Expecto"
      let result = hello "Expecto"
      Expect.equal result expected "hello did not return the correct string"
  ]
