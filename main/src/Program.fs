open System.Net.Http

// step 1: find public IP
let findPublicIP (withClient: HttpClient) =
    let ipUrl = "https://icanhazip.com"

    async {
        let! pubIp =
            withClient.GetStringAsync(ipUrl)
            |> Async.AwaitTask

        return pubIp
    }

// step 2: create a timer which checks for a new IP Address every thirty seconds
let createObservableTimer interval =
    let timer = new System.Timers.Timer(float interval)
    timer.AutoReset <- true // makes timer repeat itself after elapsing

    let observable = timer.Elapsed

    let task =
        async {
            timer.Start()
            do! Async.Sleep 120000 // run for a minute and a half; removing this will make it run indefinitely
            timer.Stop() // stop timer
        }

    (task, observable)

[<EntryPoint>]
let main _ =
    use httpClient = new HttpClient()

    let (timer, eventStream) = createObservableTimer 1000

    eventStream
    |> Observable.subscribe (fun _ -> printfn "tick %A" System.DateTime.Now)
    |> ignore

    0
