open System.Net.Http
open Thoth.Json.Net

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
    let testJson =
        "{'status':'SUCCESS', 'yourIp':'192.168.1.1'}"

    Decode.fromString Domain.DNSRecord.PBPingResponse.decoder testJson
    |> (printfn "%A")

    0
