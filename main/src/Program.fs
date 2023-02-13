open System.Net.Http

open Functions.EditDNSRecord
open Domain.EditDNSRecord

[<EntryPoint>]
let main _ =
    use client = new HttpClient()
    let secretKey = Secrets.PBAPISecretKey
    let apiKey = Secrets.PBAPIKey

    let pingCmd: Domain.Ping.PBPingCommand =
        { APIKey = apiKey
          SecretAPIKey = secretKey }

    // let bodyParams =
    //     { SecretAPIKey = secretKey
    //       APIKey = apiKey
    //       Name = None
    //       Type = "A"
    //       Content = "8.8.8.8"
    //       TTL = None
    //       Prio = None }

    // let urlParams =
    //     { Domain = "noah-hood.io"
    //       Subdomain = None }

    // let cmd =
    //     { BodyParams = bodyParams
    //       URLParams = urlParams }

    // async {
    //     let! result = editRecord client cmd

    //     printfn "%A" result
    // }
    // |> Async.RunSynchronously

    async {
        let! pingResponse = Functions.Ping.fetchIP client pingCmd
        printfn "%A" pingResponse
    }
    |> Async.RunSynchronously

    0
