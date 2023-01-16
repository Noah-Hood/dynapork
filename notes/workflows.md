# Bounded Context: IP Monitoring

Workflow: Retrieve IP
triggered by:
"Timer Window Expired" event
primary input:
PorkBun Credentials
output events:
"Public IP Received" event

Workflow: Validate IP Address
triggered by:
"Public IP Address Retrieved" event
primary input:
New IP Address
output events:
"New IP Address Validated" event

Workflow: Determine if New IP Same as Old IP
triggered by:
"New IP Address Validated" event
primary input:
Last Known Public IP Address (verified)
other input:
Newly-Queried Public IP Address (verified)
output events:
"Public IP Changed" event

Workflow: Update Last Known Public IP Address
triggered by:
"Public IP Changed" event
primary input:
New Public IP
output events:
"IP Address Updated"
side-effects:
last known IP Address updated

# Bounded Context: DNS Record

workflow: "Validate DNS Record"
  input: UnvalidatedDNSRecord
  output: 
    ValidatedDNSRecord
    OR InvalidDNSRecord

  // step 1:
  do ValidateTTL
  if TTL is invalid then
    InvalidateRecord
    stop

  // step 2:
  do ValidateName
  if Name is custom then
    if Name is invalid then
      InvalidateRecord
      stop