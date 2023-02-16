namespace Domain

open Domain.Ping

module IPWatcher =
    type IPWatcher() =
        let iPChanged = new Event<IPAddress>()

        [<CLIEvent>]
        member this.IPChanged = iPChanged.Publish

        member this.TriggerIPChange(address) = iPChanged.Trigger(address)
