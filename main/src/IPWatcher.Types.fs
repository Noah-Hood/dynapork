namespace Domain

open Domain.Ping

module IPWatcher =
    type IPWatcher() =
        let IPChanged = new Event<IPAddress>()

        [<CLIEvent>]
        member this.IPChanged = IPChanged.Publish

        member this.TriggerIPChange(address) = IPChanged.Trigger(address)
