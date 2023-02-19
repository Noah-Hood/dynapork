namespace Domain

open System.Net.Http
open Domain.Ping

module IPWatcher =
    type IPWatcher() =
        let iPChanged = new Event<IPAddress>()

        [<CLIEvent>]
        member this.IPChanged = iPChanged.Publish

        member this.TriggerIPChange(address) = iPChanged.Trigger(address)


    type CreateIPWatcher =
        NLog.Logger -> (unit -> Async<PBPingResult>) -> int -> Async<unit> * IEvent<Handler<IPAddress>, IPAddress>
