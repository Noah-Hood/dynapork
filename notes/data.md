# Bounded Context: IP Monitoring

data TimeWindow = Frequency of re-checking IP

data IPV4 = string of x.x.x.x where -1 < x < 256 

data IPAddress = 
  Unvalidated 
  OR Validated

data Unvalidated = IPV4
data Validated = IPV4

data IPValidationResult = 
  IPAddress
  OR IPValidationError

data IPValidationError = 
  Empty
  OR OverLength
  OR InvalidQuartets

# Bounded Context: DNS Record Maintenance

data TTL = Timespan in seconds

data DNS Listing = 
  Name
  AND Type
  AND Content
  AND TTL
  AND Prio

data Name = 
  Root
  OR Subdomain

data Subdomain = string

data DNSType = 
  A
  OR MX
  OR CNAME
  OR ALIAS
  OR TXT
  OR NS
  OR AAAA
  OR SRC
  OR TLSA
  OR CAAA

data Content = string

data Prio = unknown
