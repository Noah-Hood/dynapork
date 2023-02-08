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
    let testSuccess =
        "{'status':'SUCCESS', 'yourIp':'192.168.1.1'}"

    let testFailure =
        "{'status':'SUCCESS', 'yourIp':'192.168.1.1'}"


    Decode.fromString Domain.DNSRecord.PBPingResponse.decoder testSuccess
    |> (printfn "%A")

    printfn "%s" "\n\n\n"

    Decode.fromString Domain.DNSRecord.PBPingResponse.decoder testFailure
    |> (printfn "%A")

    0
