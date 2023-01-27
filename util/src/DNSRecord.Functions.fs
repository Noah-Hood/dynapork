namespace Functions

open Domain

module DNSRecord =
    let updateDNSRecord: UpdateDNSRecord =
        fun dnsSvc cmd ->
            match cmd with
            | UpdateRecord u ->
                let { Data = data; TimeStamp = timestamp } = u

                async {
                    let! svcResult = dnsSvc data

                    return
                        match svcResult with
                        | Ok o -> o |> DNSRecordUpdated |> Ok
                        | Error _ ->
                            RecordNotUpdated "Failed to update DNS Record"
                            |> Error
                }
            | _ ->
                async {
                    return
                        (RecordNotUpdated "Called update function with incorrect command"
                         |> Error)
                }

    let refreshTTL: RefreshTTL =
        fun svc cmd ->
            match cmd with
            | RefreshTTL r ->
                let { Command.Data = rData } = r
                let { Record = rcd; NewTTL = ttl } = rData

                let newRcd =
                    { Data = { rcd.Data with TTL = ttl }
                      TS = System.DateTime.Now }

                async {
                    let! svcResult = svc newRcd

                    return
                        match svcResult with
                        | Ok o -> Ok(o |> TTLRefreshed)
                        | Error _ ->
                            TTLNotRefreshed "Failed to refresh TTL with TTL service"
                            |> Error
                }
            | UpdateRecord u ->
                async {
                    let { Command.Data = record } = u

                    let res =
                        record
                        |> InvalidCommandError
                        |> InvalidCommand
                        |> Error

                    return res
                }
