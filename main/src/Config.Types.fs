namespace Domain

open Thoth.Json.Net

module Config =
    type DomainName = DomainName of string
    type Subdomain = Subdomain of string

    [<Measure>]
    type ms

    [<Measure>]
    type sec

    type Interval = Inverval of int<ms>
    type TTL = TTL of int<sec>
    type Priority = Priority of int

    type RecordType =
        | A
        | MX
        | CNAME
        | ALIAS
        | TXT
        | NS
        | AAAA
        | SRV
        | TLSA
        | CAA

    type DomainInfo =
        { Domain: DomainName
          Subdomain: Subdomain option
          RecordType: RecordType
          TTL: TTL option
          Prio: Priority option }

    type Configuration =
        { Interval: Interval
          Domains: seq<DomainInfo> }

    type LoadYMLConfig = string -> Configuration

    module RecordType =
        let recordTypeToString rt =
            match rt with
            | A -> "A"
            | MX -> "MX"
            | CNAME -> "CNAME"
            | ALIAS -> "ALIAS"
            | TXT -> "TXT"
            | NS -> "NS"
            | AAAA -> "AAAA"
            | SRV -> "SRV"
            | TLSA -> "TLSA"
            | CAA -> "CAA"

        let stringToRecordType (st: string) =
            match st.ToUpperInvariant() with
            | "A" -> Some A
            | "MX" -> Some MX
            | "CNAME" -> Some CNAME
            | "ALIAS" -> Some ALIAS
            | "TXT" -> Some TXT
            | "NS" -> Some NS
            | "AAAA" -> Some AAAA
            | "SRV" -> Some SRV
            | "TLSA" -> Some TLSA
            | "CAA" -> Some CAA
            | _ -> None


        let encoder (rt: RecordType) =
            rt |> recordTypeToString |> Encode.string

        let decoder: Decoder<RecordType> =
            Decode.index 0 Decode.string
            |> Decode.andThen (fun rts ->
                let res =
                    match rts with
                    | "A" -> Ok A
                    | "MX" -> Ok MX
                    | "CNAME" -> Ok CNAME
                    | "ALIAS" -> Ok ALIAS
                    | "TXT" -> Ok TXT
                    | "NS" -> Ok NS
                    | "AAAA" -> Ok AAAA
                    | "SRV" -> Ok SRV
                    | "TLSA" -> Ok TLSA
                    | "CAA" -> Ok CAA
                    | x -> Error x // if not one of these, disallowed

                match res with
                | Ok a -> a |> Decode.succeed
                | Error e -> Decode.fail $"Invalid RecordType received: {e}")
