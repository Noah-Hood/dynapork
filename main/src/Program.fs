open System.Net.Http

open Functions.Ping
open Functions.IPWatcher
open Domain.Ping
open Domain.IPWatcher


/// <summary>Creates and returns an NLog Logger, preconfigured
/// to use the console to log</summary>
let createLogger () =
    let config = new NLog.Config.LoggingConfiguration()

    let logConsole =
        new NLog.Targets.ColoredConsoleTarget("logconsole")

    config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logConsole)

    NLog.LogManager.Configuration <- config

    let logger = NLog.LogManager.GetCurrentClassLogger()

    logger


/// <summary>Wraps fetchIP in a function which bakes-in the
/// HTTPClient and command (credentials) for pinging</summary>
let createIPService client cmd =
    fun () -> async { return! fetchIP client cmd }

[<EntryPoint>]
let main _ =
    use client = new HttpClient()
    let secretKey = Secrets.PBAPISecretKey
    let apiKey = Secrets.PBAPIKey

    printfn "%s" apiKey
    printfn "%s" secretKey

    // let pingCmd: Domain.Ping.PBPingCommand =
    //     { APIKey = apiKey
    //       SecretAPIKey = secretKey }

    // let ipsvc = createIPService client pingCmd

    // let logger = createLogger ()

    // let task, observable = createIPWatcher logger ipsvc 10000

    // observable
    // |> Observable.subscribe (fun e -> logger.Info($"Updating ip DNS record to match {e}"))
    // |> ignore

    // task |> Async.RunSynchronously

    0
