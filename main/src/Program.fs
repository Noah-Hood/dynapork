open System.Net.Http
open Thoth.Json.Net

open DNSRecord.Ping
open DNSRecord.EditDNSRecord

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

    let sampleRq =
        "{'secretapikey':'secret', 'apikey':'notsecret', 'name':'www', 'type':'A', 'content': '192.168.1.1', 'ttl': '600'}"

    let sampleObj =
        { SecretAPIKey = "secret"
          APIKey = "notsecret"
          Name = None
          Type = "A"
          Content = "192.168.1.1"
          TTL = None
          Prio = None }

    Decode.fromString EditDNSRecordCommand.decoder sampleRq
    |> (printfn "%A")

    EditDNSRecordCommand.encoder sampleObj
    |> (fun x -> x.ToString())
    |> (printfn "%s")



    0
