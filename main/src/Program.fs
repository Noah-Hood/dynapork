open System.Net.Http
open Thoth.Json.Net

open Functions.Ping


[<Literal>]
let PorkBunEditDNSURL = "https://porkbun.com/api/json/v3/edit"

[<EntryPoint>]
let main _ =
    use client = new HttpClient()

    async {
        let! ipaddressResult = fetchIP client { SecretAPIKey = ""; APIKey = "" }

        printfn "%A" ipaddressResult
    }
    |> Async.RunSynchronously

    0
