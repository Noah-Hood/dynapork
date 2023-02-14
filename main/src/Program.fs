open System.Net.Http

open Functions.EditDNSRecord
open Domain.EditDNSRecord
open Domain.Ping
open Domain.IPWatcher

let createIPWatcher ipService interval =
    let watcher = new IPWatcher()
    let mutable ipAddress = IPAddress ""

    let observable = watcher.IPChanged

    let task =
        async {
            while true do
                let! newestIP = ipService

                if newestIP <> ipAddress then
                    watcher.TriggerIPChange(newestIP)
                    ipAddress <- newestIP

                do! Async.Sleep(float interval)
        }

    (task, observable)

[<EntryPoint>]
let main _ =
    use client = new HttpClient()
    let secretKey = Secrets.PBAPISecretKey
    let apiKey = Secrets.PBAPIKey

    printfn "apikey: %s" apiKey
    printfn "secret: %s" secretKey

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
