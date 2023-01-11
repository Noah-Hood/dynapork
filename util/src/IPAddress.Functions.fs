namespace Functions

module IPAddress =
    open Domain

    let validateIP: ValidateIP =
        fun (UnvalidatedIP unValStr) -> unValStr |> IPV4 |> ValidatedIP |> Ok
