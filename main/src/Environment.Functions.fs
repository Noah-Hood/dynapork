namespace Functions

open Microsoft.Extensions.Configuration
open System

open Domain.Environment

module Environment =
    let loadProgramEnvironment: LoadProgramEnvironment =
        let failMsg =
            "Environment variable 'ENVIRONMENT' must be set to one of 'dev' or 'prod.'"

        fun s ->
            try
                match s.ToLowerInvariant() with
                | "development"
                | "dev"
                | "develop" -> DEV
                | "production"
                | "prod" -> PROD
                | _ -> failwith failMsg
            with
            | :? NullReferenceException -> failwith failMsg

    let private addEnvVars env (cfgBldr: IConfigurationBuilder) = cfgBldr.AddEnvironmentVariables()

    let private addRuntimeSecrets env (cfgBldr: IConfigurationBuilder) =
        match env with
        | DEV -> cfgBldr.AddUserSecrets<Credentials>()
        | PROD -> cfgBldr.AddKeyPerFile("/var/secrets", true)

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

            let apiKey =
                match tryGetPartial "APIKEY" with
                | Some a -> a |> APIKey
                | None -> failwith "APIKEY environment variable must be set."

            let secretKey =
                match tryGetPartial "SECRETKEY" with
                | Some s -> s |> SecretKey
                | None -> failwith "SECRETKEY environment variable must be set."

            { APIKey = apiKey
              SecretKey = secretKey }
