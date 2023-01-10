# Bounded Context: Information Gathering

  data IPV4Address = four groups of string separated with periods

  data InvalidIP = IPV4Address

  data ValidIP = IPV4Address

  data PublicIP = 
    InvalidIP
    OR ValidIP

  data DNSRecord = 
    ID
    AND Name
    AND DNSRecordType
    AND Content // IPv4
    AND TTL
    AND Priority
    AND Notes

  data ID = string

  data Name = string

  data DNSRecordData = 
    A
    OR MX
    OR CNAME
    OR ALIAS
    OR TXT
    OR NS
    OR AAAA
    OR SRV
    OR TLSA
    OR CAA

  data Content = IPV4Address

  data TTL = integer time in seconds, greater than or equal to 600

  data Priority = unknown

  data Notes = string

  data DNSRecord = 
    InvalidatedDNSRecord
    OR ValidDNSRecord
