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
