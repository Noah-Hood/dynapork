module EnvironmentFunctionsTests

open Expecto
open Functions.Environment
open Domain.Environment

[<Tests>]
let loadProgramEnvironmentTests =
    testList
        "loadProgramEnvironment test scenarios"
        [ testCase "returns DEV for 'development'"
          <| fun _ ->
              let result = loadProgramEnvironment "development"

              Expect.equal result DEV "did not return DEV when provided 'development'"

          testCase "returns DEV for 'develop'"
          <| fun _ ->
              let result = loadProgramEnvironment "develop"

              Expect.equal result DEV "did not return DEV when provided 'develop'"

          testCase "returns DEV for 'dev'"
          <| fun _ ->
              let result = loadProgramEnvironment "dev"

              Expect.equal result DEV "did not return DEV when provided 'dev'"

          testCase "returns DEV for 'DEVELOPMENT'"
          <| fun _ ->
              let result = loadProgramEnvironment "DEVELOPMENT"

              Expect.equal result DEV "did not return DEV when provided 'DEVELOPMENT'"

          testCase "returns DEV for 'DEVELOP'"
          <| fun _ ->
              let result = loadProgramEnvironment "DEVELOP"

              Expect.equal result DEV "did not return DEV when provided 'DEVELOP'"

          testCase "returns DEV for 'DEV'"
          <| fun _ ->
              let result = loadProgramEnvironment "DEV"

              Expect.equal result DEV "did not return DEV when provided 'DEV'"


          ]
