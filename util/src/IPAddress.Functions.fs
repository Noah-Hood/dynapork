namespace Functions

module IPAddress =
    open Domain

    let updateIP: UpdateIP =
        fun (cmd) (IPAddress oldIPData) ->
            match cmd with
            | UpdateIP (IPAddress newIPData) ->
                let { Address = newAddr
                      UpdatedAt = newUpdatedAt } =
                    newIPData

                let { Address = oldAddr
                      UpdatedAt = oldUpdatedAt } =
                    oldIPData

                let newIP =
                    { Address = newAddr
                      UpdatedAt = newUpdatedAt }
                    |> IPAddress

                match (newAddr = oldAddr, newUpdatedAt = oldUpdatedAt) with
                | true, true -> newIP |> Unchanged
                | true, false -> newIP |> Revalidated
                | _ -> newIP |> Changed
