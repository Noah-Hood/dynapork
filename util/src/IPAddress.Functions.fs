namespace Functions

open System.Text.RegularExpressions

module IPAddress =
    open Domain

    let private ipRegex =
        @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$"

    let validateIP: ValidateIP =
        fun (UnvalidatedIP ipStr) ->
            if ipStr = "" then
                Empty "IP Address was empty" |> Error
            elif not (Regex.IsMatch(ipStr, ipRegex)) then
                InvalidQuartets "IP Address is malformed" |> Error
            else
                ipStr |> ValidatedIP |> Ok
