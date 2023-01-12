namespace Functions

open Domain

module PublicIP =
    let updatePublicIP: UpdatePublicIP =
        fun pIP ipAddr ->
            match ipAddr with
            | Unvalidated _ -> None
            | Validated _ ->
                match pIP with
                | Empty ->
                    { Address = ipAddr
                      LastValidated = System.DateTime.Now }
                    |> Current
                | Current c ->
                    { c with
                        Address = ipAddr
                        LastValidated = System.DateTime.Now }
                    |> Current
                |> PublicIPUpdated
                |> Some
