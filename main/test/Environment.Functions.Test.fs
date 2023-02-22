module EnvironmentFunctionsTests

open Expecto
open Functions.Environment
open Domain.Environment

let config =
    { FsCheckConfig.defaultConfig with maxTest = 10000 }

[<Tests>]
let loadProgramEnvironmentTests =
    testList
        "loadProgramEnvironment tests"
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

          testCase "returns PROD for 'production"
          <| fun _ ->
              let result = loadProgramEnvironment "production"

              Expect.equal result PROD "did not return PROD when provided 'production'"

          testCase "returns PROD for 'prod'"
          <| fun _ ->
              let result = loadProgramEnvironment "prod"

              Expect.equal result PROD "did not return PROD when provided 'prod'"

          testCase "returns PROD for 'PRODUCTION'"
          <| fun _ ->
              let result = loadProgramEnvironment "PRODUCTION"

              Expect.equal result PROD "did not return PROD when provided 'PRODUCTION'"

          testCase "returns PROD for 'PROD'"
          <| fun _ ->
              let result = loadProgramEnvironment "PROD"

              Expect.equal result PROD "did not return PROD when provided 'PROD'"

          testCase "fails for invalid string"
          <| fun _ ->
              Expect.throws
                  (fun _ -> loadProgramEnvironment "prodvelopment" |> ignore)
                  "did not throw an exception when provided an invalid value"

          testPropertyWithConfig config "returns only one of three possible results, irrespective of input"
          <| fun s ->
              try
                  let result = loadProgramEnvironment s

                  Expect.contains
                      [| DEV; PROD |]
                      result
                      "success did not return one of two acceptable success conditions"
              with
              | ex ->
                  Expect.equal
                      ex.Message
                      "Environment environment variable must be set to one of 'dev' or 'prod.'"
                      "did not return expected error for invalid input"

          testPropertyWithConfig config "returns the same result twice for the same input"
          <| fun s ->
              try
                  let r1 = loadProgramEnvironment s
                  let r2 = loadProgramEnvironment s

                  Expect.equal r1 r2 "did not return the same result twice for the same input"
              with
              | ex ->
                  Expect.equal
                      ex.Message
                      "Environment environment variable must be set to one of 'dev' or 'prod.'"
                      "did not return expected error for invalid input"

          ]
