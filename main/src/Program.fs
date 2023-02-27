open System.Net.Http

open Functions.Environment
open Functions.EditDNSRecord
open Functions.Ping
open Functions.IPWatcher
open Domain.Ping
open Domain.EditDNSRecord
open Domain.Environment


/// <summary>Creates and returns an NLog Logger, configured
/// to use the console</summary>
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
    let environment =
        System.Environment.GetEnvironmentVariable("ENVIRONMENT")
        |> loadProgramEnvironment

    let { Credentials = credentials
          DomainInfo = domainInfo } =
        loadEnvironment environment

    use client = new HttpClient()
    let logger = createLogger ()

    let ipSvc = createIPService client credentials

    let (task, observable) =
        createIPWatcher logger ipSvc (15 * 60 * 1000) // check ip every 15 mins

    observable
    |> Observable.subscribe (fun (IPAddress x) ->
        logger.Info($"Updating DNS Record with new IP: {x}...")

        let urlParams: URLParams =
            { Domain = domainInfo.Domain
              RecordType = domainInfo.RecordType
              Subdomain = domainInfo.Subdomain }

        let bodyParams: BodyParams =
            { Content = IPAddress x
              TTL = Some 600
              Prio = None }

        let editCmd =
            { Credentials = credentials
              BodyParams = bodyParams
              URLParams = urlParams }

        async {
            let! result = editRecord client editCmd

            match result with
            | Ok _ -> logger.Info($"Updated A record for {domainInfo.Domain} to {x} successfully.")
            | Error e -> logger.Error($"Failed to update A record for {domainInfo.Domain} to {x}: {e}")

        }
        |> Async.RunSynchronously)
    |> ignore

    task |> Async.RunSynchronously

    0
