namespace Functions

open Domain.IPWatcher
open Domain.Ping

module IPWatcher =
    /// <summary>Creates a task and observable.
    /// The task repeats every <code>interval</code> milliseconds,
    /// fetching the current public IP Address from
    /// the provided <code>ipScv</code>. It compares it to the last
    /// known IP Address, and triggers the observable
    /// if the IP Address has changed.</summary>
    let createIPWatcher: CreateIPWatcher =
        fun logger fetchIPSvc interval ->
            let watcher = new IPWatcher()
            let mutable ipAddress = IPAddress ""

            let observable = watcher.IPChanged

            let task =
                async {
                    while true do
                        let! newestIP = fetchIPSvc ()

                        match newestIP with
                        | Ok ipaddr ->
                            logger.Info($"Fetched IP Address {ipaddr}")

                            if ipaddr <> ipAddress then
                                logger.Info(
                                    $"IP Address change detected, from {ipAddress} -> {ipaddr}; triggering changed event"
                                )

                                watcher.TriggerIPChange(ipaddr)
                                ipAddress <- ipaddr
                        | Error e -> logger.Error($"Failed to fetch a valid IP Address: {e}")

                        do! Async.Sleep(int interval)
                }

            (task, observable)
