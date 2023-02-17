open Microsoft.Extensions.Configuration
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

[<CLIMutable>]
type EnvironmentVariables =
    { Environment: string
      DomainName: string }

[<EntryPoint>]
let main _ =
    let environment =
        System.Environment.GetEnvironmentVariable("environment")

    let config =
        match environment.ToLowerInvariant() with
        | "development"
        | "dev"
        | "develop" ->
            (new ConfigurationBuilder())
                .AddUserSecrets<Domain.Ping.PBPingCommand>()
                .AddEnvironmentVariables()
                .Build()
        | "production"
        | "prod" ->
            (new ConfigurationBuilder())
                .AddKeyPerFile("/var/secrets", true)
                .AddEnvironmentVariables()
                .Build()
        | _ -> failwith "'environment' environment variable must be set to one of 'production' or 'development"


    let apiKey = config.Item("APIKey")
    let secretKey = config.Item("SecretKey")

    match System.String.IsNullOrEmpty(apiKey), System.String.IsNullOrEmpty(secretKey) with
    | true, true ->
        failwith
            "Both ApiKey and SecretKey are empty; both must be set either as a User Secret for development or a Docker secret in production."
    | true, false ->
        failwith
            "ApiKey is empty; both ApiKey and SecretKey must be set either as a User Secret for development or a Docker secret in production."
    | false, true ->
        failwith
            "SecretKey is empty; both ApiKey and SecretKey must be set either as a User Secret for development or a Docker secret in production."
    | false, false -> ()

    let domainName = config.Item("DomainName")

    if System.String.IsNullOrEmpty(domainName) then
        failwith
            "DomainName environment variable is empty; must be set to the domain for which DynaPork updates the A record."

    let pingCmd: Domain.Ping.PBPingCommand =
        { APIKey = apiKey
          SecretAPIKey = secretKey }

    use client = new HttpClient()
    let logger = createLogger ()

    let ipSvc = createIPService client pingCmd

    let (task, observable) = createIPWatcher logger ipSvc 10000

    observable
    |> Observable.subscribe (fun x -> logger.Info($"Updating DNS Record with new IP: {x}"))
    |> ignore

    task |> Async.RunSynchronously

    0
