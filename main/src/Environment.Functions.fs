namespace Functions

open Microsoft.Extensions.Configuration
open System

open Domain.Environment

module Environment =
    [<Literal>]
    let private DEFAULT_INTERVAL = 900000

    [<Literal>]
    let private MINIMUM_INTERVAL = 5000


    let loadProgramEnvironment: LoadProgramEnvironment =
        fun s ->
            try
                match s.ToLowerInvariant() with
                | "development"
                | "dev"
                | "develop" -> DEV
                | "production"
                | "prod" -> PROD
                | _ -> failwith "Environment environment variable must be set to one of 'dev' or 'prod.'"
            with
            | :? NullReferenceException ->
                failwith "Environment environment variable must be set to one of 'dev' or 'prod.'"

    let private addEnvVars env (cfgBldr: IConfigurationBuilder) = cfgBldr.AddEnvironmentVariables()

    let private addRuntimeSecrets env (cfgBldr: IConfigurationBuilder) =
        match env with
        | DEV -> cfgBldr.AddUserSecrets<EnvironmentVariables>()
        | PROD -> cfgBldr.AddKeyPerFile(directoryPath = "/run/secrets", optional = false)

    let private tryGetItemFromConfig (config: IConfigurationRoot) (itemName: string) =
        let rawKey = config.Item(itemName)

        match String.IsNullOrEmpty(rawKey) with
        | true -> None
        | false -> Some rawKey


    let loadEnvironment: LoadEnvironment =
        fun env ->
            let config =
                (new ConfigurationBuilder())
                |> addEnvVars env
                |> addRuntimeSecrets env
                |> (fun x -> x.Build())

            let tryGetPartial = tryGetItemFromConfig config

            // public key for PorkBun API
            let apiKey =
                match tryGetPartial "APIKEY" with
                | Some a -> a |> APIKey
                | None -> failwith "APIKEY environment variable must be set."

            // private key for PorkBun API
            let secretKey =
                match tryGetPartial "SECRETKEY" with
                | Some s -> s |> SecretKey
                | None -> failwith "SECRETKEY environment variable must be set."

            // the domainName for which to maintain the record
            let domainName =
                match tryGetPartial "DOMAINNAME" with
                | Some d -> d |> DomainName
                | None -> failwith "DOMAINNAME environment variable must be set."

            // the optional subdomain for which to maintain the record
            let subdomain =
                match tryGetPartial "SUBDOMAIN" with
                | Some s -> s |> Subdomain |> Some
                | None -> None

            // the type of record to maintain for the domain/subdomain combo
            let recordType =
                match tryGetPartial "RECORDTYPE" with
                | Some r ->
                    match r |> RecordType.stringToRecordType with
                    | Some rt -> rt
                    | None ->
                        failwith
                            "RECORDTYPE environment variable set to invalid value. Must be one of A, MX, CNAME, ALIAS, TXT, Ns, AAAA, ETV, TLSA, or CAA."
                | None -> failwith "RECORDTYPE environment variable must be set."

            // the time (in milliseconds) between checks for a change in public IP address
            // limited to a minimum of 5 seconds
            let interval =
                match tryGetPartial "INTERVAL" with
                | Some i ->
                    let success, intVal = System.Int32.TryParse(i)

                    if success then
                        max intVal MINIMUM_INTERVAL
                    else
                        DEFAULT_INTERVAL
                | None -> DEFAULT_INTERVAL
                |> fun intVal -> intVal * 1<ms>

            let credentials =
                { APIKey = apiKey
                  SecretKey = secretKey }

            let domainInfo =
                { Domain = domainName
                  Subdomain = subdomain
                  RecordType = recordType }

            { Credentials = credentials
              DomainInfo = domainInfo
              Interval = interval }
