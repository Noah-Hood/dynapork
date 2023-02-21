module EnvironmentFunctionsTests

open Expecto
open Functions.Environment
open Domain.Environment

[<Tests>]
let loadProgramEnvironmentTests =
    testList
        "loadProgramEnvironment test scenarios"
        [ testCase "returns 'dev' for development"
          <| fun _ ->
              let result = loadProgramEnvironment "development"

              Expect.equal result DEV "did not return DEV when provided 'development'" ]
