namespace Functions

open System.Net.Http
open System.Text
open Thoth.Json.Net

module Common =
    let httpContentToString (httpContent: HttpContent) =
        async { return! httpContent.ReadAsStringAsync() |> Async.AwaitTask }

    let jsonToStringContent (jsonValue: JsonValue) =
        new StringContent(jsonValue.ToString(), Encoding.UTF8, "application/json")
