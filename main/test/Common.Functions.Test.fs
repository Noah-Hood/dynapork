module CommonFunctionsTests

open Expecto
open FsCheck
open Functions.Common
open System.Net.Http
open System.Text
open Thoth.Json.Net

let createStringContent s =
    new StringContent(s, Encoding.UTF8, "application/json")

let stringContentGen: Gen<StringContent> =
    gen {
        let! s = Arb.generate<FsCheck.NonEmptyString>

        return new StringContent(string s, Encoding.UTF8, "application/json")
    }

let jsonValueGen: Gen<JsonValue> =
    gen {
        let! s = Arb.generate<string>

        return Encode.string s
    }

type StringContentGenerator() =
    static member StringContent() = Arb.fromGen stringContentGen
    static member JsonValue() = Arb.fromGen jsonValueGen

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

[<Tests>]
let jsonToStringContentTests =
    testList
        "jsonToStringContent tests"
        [

          testPropertyWithConfig config "round-trips correctly"
          <| fun (jv: JsonValue) ->
              let result =
                  jv
                  |> jsonToStringContent
                  |> (fun sc ->
                      sc.ReadAsStringAsync()
                      |> Async.AwaitTask
                      |> Async.RunSynchronously)
                  |> Encode.string

              Expect.equal (result.ToString()) (jv.ToString()) "did not return the same JSON value provided"

          testPropertyWithConfig config "returns the same result for the same input"
          <| fun (js: JsonValue) ->
                let results = 
                    [js; js ]
                    |> List.map jsonToStringContent
                    |> List.map (fun s -> s.ReadAsStream())

                Expect.streamsEqual results[0] results[1] "did not return the same value twice"

         ]
