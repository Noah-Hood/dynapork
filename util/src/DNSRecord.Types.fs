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

/// DNSRecord creation types
type EmptyCustomNameError = EmptyCustomNameError
type InvalidTTLError = InvalidTTLError

type DNSRecordValidationError =
    | EmptyCustomName of EmptyCustomNameError
    | InvalidTTL of InvalidTTLError

type DNSRecordValidationResult = Result<DNSRecord, DNSRecordValidationError>

/// DNSRecord Commands
type UpdateRecordCmd = Command<DNSRecord>

type RefreshTTLData = { Record: DNSRecord; NewTTL: TTL }
type RefreshTTLCmd = Command<RefreshTTLData>

type DNSRecordCommand =
    | UpdateRecord of UpdateRecordCmd
    | RefreshTTL of RefreshTTLCmd

/// DNSRecord Events
type DNSRecordUpdated = DNSRecordUpdated of DNSRecord
type TTLRefreshed = TTLRefreshed of DNSRecord

/// DNSRecord Errors
type InvalidCommandError = InvalidCommandError of DNSRecord

type DNSRecordError =
    | RecordNotUpdated of string
    | TTLNotRefreshed of string
    | InvalidCommand of InvalidCommandError

type DNSRecordServiceError = | FailedToLoad

type DNSRecordService = DNSRecord -> Async<Result<DNSRecord, DNSRecordServiceError>>

type UpdateDNSRecord = DNSRecordService -> DNSRecordCommand -> Async<Result<DNSRecordUpdated, DNSRecordError>>

type RefreshTTL = DNSRecordService -> DNSRecordCommand -> Async<Result<TTLRefreshed, DNSRecordError>>

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
