module Secrets

open Microsoft.Extensions.Configuration

let config =
    (new ConfigurationBuilder())
        .AddUserSecrets<DNSRecord.Ping.PBPingFailureResponse>()
        .Build()

let PBAPIKey = config.Item("pbapikey")
let PBAPISecretKey = config.Item("pbapisecretkey")
