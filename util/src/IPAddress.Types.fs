namespace Domain

type UnvalidatedIP = UnvalidatedIP of string
type ValidatedIP = ValidatedIP of string

type IPAddress =
    | Unvalidated of UnvalidatedIP
    | Validated of ValidatedIP

type IPValidationError =
    | Empty
    | InvalidQuartets

type IPValidationResult = Result<IPAddress, IPValidationError>

type ValidateIP = UnvalidatedIP -> IPValidationResult

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
