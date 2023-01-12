namespace Domain

type PublicIPData =
    { Address: IPAddress
      LastValidated: System.DateTime }

type PublicIP =
    | Empty
    | Current of PublicIPData

type PublicIPUpdated = PublicIPUpdated of PublicIP

type UpdatePublicIP = PublicIP -> IPAddress -> PublicIPUpdated option

module PublicIP =
    let create (ip: IPAddress) =
        match ip with
        | Unvalidated _ -> Empty
        | Validated _ ->
            { Address = ip
              LastValidated = System.DateTime.Now }
            |> Current

    let value pIP =
        match pIP with
        | Empty -> None
        | Current c -> Some c
