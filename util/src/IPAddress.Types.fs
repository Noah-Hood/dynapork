namespace Domain

open System.Text.RegularExpressions

type IPAddressData =
    { Address: string
      UpdatedAt: System.DateTime }

type IPAddress = IPAddress of IPAddressData

type IPValidationError =
    | Empty
    | Malformed

type IPCommand = UpdateIP of IPAddress

type IPAddressUpdated =
    | Unchanged of IPAddress
    | Changed of IPAddress

type UpdateIP = IPCommand -> IPAddress -> IPAddressUpdated

module IPAddress =
    [<Literal>]
    let private regexPattern =
        @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$"

    /// <summary>Constructor for IPAddress type</summary>
    let create (str: string) =
        if str = "" then
            Empty |> Error
        elif not <| Regex.IsMatch(str, regexPattern) then
            Malformed |> Error
        else
            { Address = str
              UpdatedAt = System.DateTime.Now }
            |> IPAddress
            |> Ok

    /// <summary>Extracts the string content of an IPAddress</summary>
    let value (IPAddress ipStr) = ipStr
