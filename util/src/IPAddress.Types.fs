namespace Domain

// General IP Address type
type IPAddressData =
    { Address: string
      UpdatedAt: System.DateTime }

type IPAddress = IPAddress of IPAddressData

// Command(s)
type UpdateIPCmd = Command<IPAddress>

// Events
type IPAddressEvent =
    | Unchanged of IPAddress
    | Revalidated of IPAddress
    | Changed of IPAddress

// Errors
type IPValidationError =
    | Empty
    | Malformed

// UpdateIP workflow
type UpdateIP = UpdateIPCmd -> IPAddress -> IPAddressEvent

module IPAddress =
    open System.Text.RegularExpressions

    [<Literal>]
    let private regexPattern =
        @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$"

    /// <summary>Constructor for IPAddress type</summary>
    let create (str: string) =
        if str = "" then // cannot be blank
            Empty |> Error
        elif not <| Regex.IsMatch(str, regexPattern) then // must match generic ip regex
            Malformed |> Error
        else
            { Address = str
              UpdatedAt = System.DateTime.Now }
            |> IPAddress
            |> Ok

    /// <summary>Extracts the string content of an IPAddress</summary>
    let value (IPAddress ipStr) = ipStr
