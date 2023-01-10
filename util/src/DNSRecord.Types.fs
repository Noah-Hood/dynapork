namespace Util.Types

module DNSRecord = 
  type ID = ID of string
  type Name = Name of string
  
  type DNSRecordType = 
    | A
    | MX
    | CNAME
    | ALIAS
    | TXT
    | NS
    | AAAA
    | SRC
    | TLSA
    | CAA

  type TTL = TTL of int

  type DNSData = {
    ID: ID
    Name: Name
    DNSRecordType: DNSRecordType
    Content: IPAddress.IPV4Address
    TTL: TTL
    Priority: string
    Notes: string
  }

  type ValidDNSRecord = ValidDNSRecord of DNSData
  type InvalidatedDNSRecord = InvalidatedDNSRecord of DNSData

  type DNSRecord = 
    | ValidDNSRecord
    | InvalidatedDNSRecord
