namespace Functions

open Microsoft.Extensions.Configuration

open Domain.Environment

module Environment =
    let loadEnvironment: LoadEnvironment =
        fun (ProgramEnvironment env) ->
            let config =
                match env.ToLowerInvariant() with
                | "development"
                | "dev"
                | "develop" ->
                    (new ConfigurationBuilder())
                        .AddUserSecrets<EnvironmentVariables>()
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

            let subdomain =
                match config.Item("Subdomain") with
                | _ when System.String.IsNullOrEmpty(config.Item("subdomain")) -> None
                | x -> x |> Subdomain |> Some

            if System.String.IsNullOrEmpty(domainName) then
                failwith
                    "DomainName environment variable is empty; must be set to the domain for which DynaPork updates the A record."

            let credentials =
                { APIKey = apiKey |> APIKey
                  SecretKey = secretKey |> SecretKey }

            { Credentials = credentials
              EnvironmentVariables.Domain = domainName |> DomainName
              EnvironmentVariables.Subdomain = subdomain
              EnvironmentVariables.Environment = (ProgramEnvironment env) }
