namespace Domain

[<Measure>]
type Second

type Name =
    | Root
    | Wildcard
    | Custom of string

type DNSRecordType =
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

type Content = Domain.IPAddress
type TTL = int<Second>

type DNSRecordData =
    { Name: Name
      DNSRecordType: DNSRecordType
      Content: Content
      TTL: TTL }

type DNSRecord =
    { Data: DNSRecordData
      TS: System.DateTime }

type EmptyCustomNameError = EmptyCustomNameError
type InvalidTTLError = InvalidTTLError

type DNSRecordValidationError =
    | EmptyCustomName of EmptyCustomNameError
    | InvalidTTL of InvalidTTLError

type DNSRecordValidationResult = Result<DNSRecord, DNSRecordValidationError>


module DNSRecord =

    let validateName dnsRecordData =
        let { Name = name } = dnsRecordData

        match name with
        | Custom c ->
            if c = "" then
                EmptyCustomNameError
                |> Error
                |> Result.mapError EmptyCustomName
            else
                Ok dnsRecordData
        | _ -> Ok dnsRecordData

    let validateTTL dnsRecordData =
        let { TTL = ttl } = dnsRecordData

        match ttl with
        | _ when ttl < 600<Second> ->
            InvalidTTLError
            |> Error
            |> Result.mapError InvalidTTL
        | _ -> Ok dnsRecordData

    let validateDnsRecordData = validateName >> Result.bind validateTTL

    let create data =
        data
        |> validateDnsRecordData
        |> Result.bind (fun x ->
            Ok
                { DNSRecord.Data = x
                  DNSRecord.TS = System.DateTime.Now })
