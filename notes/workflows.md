# Bounded Context: IP Address Monitoring

  Workflow: Retrieve Local IP
    triggered by:
      "Timer Expired" event
    primary input:
      None
    output events:
      "Public IP Received" event

  Workflow: Determine if New IP Same as Old IP
    triggered by:
      "Local Public IP Received" event
    primary input:
      Last Known Public IP Address
    other input:
      Newly-Queried Public IP Address
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
      Update last known IP Address


# Bounded Context: DNS Record Monitoring

  Workflow: Retrieve DNS Record Information
    triggered by:
      "Time Expired" event
    primary input:
      PorkBun Credentials
    output events:
      "DNS Record Information Received" event

  Workflow: Invalidate DNS Record
    triggered by:
      "Public IP Changed" event
    primary input:
      New IP Address
    other input:
      Last Known DNS Record Information
    output events:
      "DNS Record Invalidated" event

  Workflow: Refresh DNS Record TTL
    triggered by:
      "DNS Record TTL Expired" event
    primary input:
      Last known DNS Record information
    other input:
      PorkBun credentials
    output events:
      "DNS Record TTL Refreshed"
    side-effects:
      Refreshing of the DNS Record's TTL




# Bounded-Context: Information Gathering

  
  Workflow: Update DNS Record
    triggered by:
      "DNS Record Invalidated" event
    primary input:
      New Public IP
    other input:
      PorkBun credentials
    output events:
      "DNS Record Updated" event
    side-effects:
      DNS record updated on PorkBun API

  Workflow: Refresh DNS Record Lease
    triggered by:
      "DNS Record Lease Expired" event
    primary input:
      Latest Known DNS Information
    other input:
      Desired lease refresh time
    output events:
      DNS Record TTL Refreshed
