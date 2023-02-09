open System.Net.Http

open Functions.Ping

[<EntryPoint>]
let main _ =
    use client = new HttpClient()
    let secretKey = Secrets.PBAPISecretKey
    let apiKey = Secrets.PBAPIKey

    async {
        let! ipaddressResult =
            fetchIP
                client
                { SecretAPIKey = secretKey
                  APIKey = apiKey }

        printfn "%A" ipaddressResult
    }
    |> Async.RunSynchronously

    0
