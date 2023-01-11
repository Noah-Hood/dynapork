namespace Domain

type UnvalidatedIP = UnvalidatedIP of string
type ValidatedIP = ValidatedIP of string

type IPAddress =
    | Unvalidated of UnvalidatedIP
    | Validated of ValidatedIP

type IPValidationError =
    | Empty of string
    | InvalidQuartets of string

type ValidateIP = UnvalidatedIP -> Result<ValidatedIP, IPValidationError>

module IPAddress =
    /// <summary>Constructor for IPAddress type</summary>
    let create (str: string) = str |> UnvalidatedIP |> Unvalidated

    /// <summary>Extracts the string content of an IPAddress</summary>
    let value ipAddr =
        match ipAddr with
        | Unvalidated u ->
            let (UnvalidatedIP strVal) = u
            strVal
        | Validated v ->
            let (ValidatedIP strVal) = v
            strVal
