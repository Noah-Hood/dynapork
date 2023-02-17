module Secrets

open Microsoft.Extensions.Configuration

let config =
    (new ConfigurationBuilder())
        .AddUserSecrets<Domain.Ping.PBPingCommand>()
        .Build()

let PBAPIKey = config.Item("pbapikey")
let PBAPISecretKey = config.Item("pbapisecretkey")
