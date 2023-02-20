namespace Functions

open Microsoft.Extensions.Configuration
open System

open Domain.Environment

module Environment =
    let loadProgramEnvironment:LoadProgramEnvironment = 
        fun s ->
            match s.ToLowerInvariant() with
            | "development" | "dev" | "develop" -> DEV
            | "production" | "prod" -> PROD
            | _ -> failwith "Environment environment variable must be set to one of 'dev' or 'prod.'"
        
    let private addEnvVars env (cfgBldr: IConfigurationBuilder) = 
        cfgBldr.AddEnvironmentVariables()

    let private addRuntimeSecrets env (cfgBldr: IConfigurationBuilder) = 
        match env with
        | DEV -> cfgBldr.AddUserSecrets<EnvironmentVariables>()
        | PROD -> cfgBldr.AddKeyPerFile("/var/secrets", true)

    let private tryGetItemFromConfig (config: IConfigurationRoot) (itemName: string) = 
        let rawKey = config.Item(itemName)
        match String.IsNullOrEmpty(rawKey) with
        | true -> None
        | false -> Some rawKey


    let loadEnvironment: LoadEnvironment =
        fun env ->
            let config = (new ConfigurationBuilder()) |> addEnvVars env |> addRuntimeSecrets env |> (fun x -> x.Build())

            let tryGetPartial = tryGetItemFromConfig config

            let apiKey = 
                match tryGetPartial "APIKEY" with
                | Some a -> a |> APIKey
                | None -> failwith "APIKEY environment variable must be set."

            let secretKey = 
                match tryGetPartial "SECRETKEY" with
                | Some s -> s |> SecretKey
                | None -> failwith "SECRETKEY environment variable must be set."

            let domainName = 
                match tryGetPartial "DOMAINNAME" with
                | Some d -> d |> DomainName
                | None -> failwith "DOMAINNAME environment variable must be set."

            let recordType = 
                match tryGetPartial "RECORDTYPE" with
                | Some r -> 
                    match r |> RecordType.stringToRecordType with
                    | Some rt -> rt
                    | None -> failwith "RECORDTYPE environment variable set to invalid value. Must be onf of A, MX, CNAME, ALIAS, TXT, Ns, AAAA, ETV, TLSA, or CAA."
                | None -> failwith "RECORDTYPE environment variable must be set."

            let subdomain = 
                match tryGetPartial "SUBDOMAIN" with    
                | Some s -> s |> Subdomain |> Some
                | None -> None

            let credentials =
                { APIKey = apiKey
                  SecretKey = secretKey}

            let domainInfo = {
                Domain = domainName
                Subdomain = subdomain
                RecordType = recordType
            }

            { Credentials = credentials
              DomainInfo = domainInfo }
