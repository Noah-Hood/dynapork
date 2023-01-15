namespace Domain

[<Measure>]
type Second

type SecretAPIKey = SecretAPIKey of string
type APIKey = APIKey of string

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

type DNSRecordCommand =
    | RefreshTTL of TTL
    | UpdateRecord of DNSRecordData

type DNSRecordEvent =
    | Unchanged
    | TTLRefreshed of DNSRecordData
    | DNSRecordUpdated of DNSRecordData
