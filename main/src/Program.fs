open System.Net.Http

open Functions.EditDNSRecord
open Functions.Ping
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
                printfn "Fetching new IP..."
                let! newestIP = ipService ()
                printfn "New ip: %A" newestIP

                match newestIP with
                | Ok ipaddr ->
                    if ipaddr <> ipAddress then
                        printfn "Triggering IP Change..."
                        watcher.TriggerIPChange(ipaddr)
                        ipAddress <- ipaddr
                | Error e -> printfn "failed to fetch IP: %A" e

                do! Async.Sleep(int interval)
        }

    (task, observable)

let createIPService client cmd =
    fun () -> async { return! fetchIP client cmd }

[<EntryPoint>]
let main _ =
    use client = new HttpClient()
    let secretKey = Secrets.PBAPISecretKey
    let apiKey = Secrets.PBAPIKey

    let pingCmd: Domain.Ping.PBPingCommand =
        { APIKey = apiKey
          SecretAPIKey = secretKey }

    let ipsvc = createIPService client pingCmd

    let task, observable = createIPWatcher ipsvc 10000

    observable
    |> Observable.subscribe (fun e -> printfn "Updating DNS Record: %A" e)
    |> ignore

    task |> Async.RunSynchronously

    0
