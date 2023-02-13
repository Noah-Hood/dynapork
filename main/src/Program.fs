open System.Net.Http

open Functions.EditDNSRecord
open Domain.EditDNSRecord


[<EntryPoint>]
let main _ =
    use client = new HttpClient()
    let secretKey = Secrets.PBAPISecretKey
    let apiKey = Secrets.PBAPIKey

    let bodyParams =
        { SecretAPIKey = secretKey
          APIKey = apiKey
          Name = None
          Type = "A"
          Content = "8.8.8.8"
          TTL = None
          Prio = None }

    let urlParams =
        { Domain = "noah-hood.io"
          Subdomain = None }

    let cmd =
        { BodyParams = bodyParams
          URLParams = urlParams }

    async {
        let! result = editRecord client cmd

        printfn "%A" result
    }
    |> Async.RunSynchronously

    0
