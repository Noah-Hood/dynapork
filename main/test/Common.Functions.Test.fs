module CommonFunctionsTests

open Expecto
open FsCheck
open Functions.Common
open Domain.Common
open System.Net.Http
open System.Text

let createStringContent s =
    new StringContent(s, Encoding.UTF8, "application/json")

let stringContentGen: Gen<StringContent> =
    gen {
        let! s = Arb.generate<FsCheck.NonEmptyString>

        return new StringContent(string s, Encoding.UTF8, "application/json")
    }

type StringContentGenerator() =
    static member StringContent() = Arb.fromGen stringContentGen

let config =
    { FsCheckConfig.defaultConfig with
        arbitrary = [ typeof<StringContentGenerator> ]
        maxTest = 10000 }

[<Tests>]
let httpContentToStringTests =
    testList
        "httpContentToString tests"
        [

          testPropertyWithConfig config "round-trips correctly"
          <| fun (s: StringContent) ->
              let result =
                  s
                  |> httpContentToString
                  |> Async.RunSynchronously
                  |> (fun s -> new StringContent(string s, Encoding.UTF8, "application/json"))

              Expect.streamsEqual (result.ReadAsStream()) (s.ReadAsStream()) "did not rount-trip successfully"


          testPropertyWithConfig config "returns the same result for the same input"
          <| fun (sc: StringContent) ->
              let r1 =
                  sc
                  |> httpContentToString
                  |> Async.RunSynchronously

              let r2 =
                  sc
                  |> httpContentToString
                  |> Async.RunSynchronously

              Expect.equal r1 r2 "did not return the same result twice for the same input"

          ]
