open System.Net.Http
open Thoth.Json.Net

open DNSRecord.Ping

[<Literal>]
let PorkBunPingURL = "https://porkbun.com/api/json/v3/ping"


// step 1: find public IP
let findPublicIP (withClient: HttpClient) =
    let ipUrl = "https://icanhazip.com"

    async {
        let! pubIp =
            withClient.GetStringAsync(ipUrl)
            |> Async.AwaitTask

        return pubIp
    }

[<EntryPoint>]
let main _ =
    use client = new HttpClient()

    let apiKey = Secrets.PBAPIKey
    let apiSecretKey = Secrets.PBAPISecretKey

    let cmd =
        { SecretAPIKey = apiSecretKey
          APIKey = apiKey }

    let cmdArgs = PBPingCommand.encoder cmd

    let content =
        new StringContent(cmdArgs.ToString(), System.Text.Encoding.UTF8, "application/json")

    async {
        let! result =
            client.PostAsync(PorkBunPingURL, content)
            |> Async.AwaitTask

        let! content =
            result.Content.ReadAsStringAsync()
            |> Async.AwaitTask

        content
        |> Decode.fromString PBPingResponse.decoder
        |> (printfn "%A")
    }
    |> Async.RunSynchronously
    |> ignore

    0
