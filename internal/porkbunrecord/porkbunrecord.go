package porkbunrecord

import (
	"encoding/json"
	"fmt"
	"net/netip"

	"github.com/hoodnoah/dynapork/internal/httpclient"
)

const dnsRecordFetchURL = "https://api.porkbun.com/api/json/v3/dns/retrieveByNameType/%s/%s/%s" // url, type, subdomain

type InvalidAPIKeysError struct{}

func (i *InvalidAPIKeysError) Error() string {
	return "status 400: invalid API keys (001)"
}

type IPBRecord interface{}

type GenericDNSRecord struct {
	Domain string
	Host   string
	Answer netip.Addr
	Ttl    uint
}

type ARecord struct {
	DnsRecord GenericDNSRecord
	Pbauth    *PBAuth
	Client    httpclient.IHttpClient
}

// creates a new A record struct
// tries fetching the existing record's ip address to prevent re-setting the same value at launch
func NewARecord(domain string, host string, ttl uint, auth *PBAuth, client httpclient.IHttpClient) (IPBRecord, error) {
	record := ARecord{
		DnsRecord: GenericDNSRecord{
			Domain: domain,
			Host:   host,
			Answer: netip.IPv4Unspecified(),
			Ttl:    ttl,
		},
		Pbauth: auth,
		Client: client,
	}

	// try and populate the existing IP record
	address, err := record.getCurrentAnswer()
	if err != nil {
		return nil, err
	}

	record.DnsRecord.Answer = address

	return &record, nil
}

// fetches the current answer for a given IP record.
func (a *ARecord) getCurrentAnswer() (netip.Addr, error) {
	// fetch existing record information
	url := fmt.Sprintf(dnsRecordFetchURL, a.DnsRecord.Domain, "A", a.DnsRecord.Host)
	var dnsRetrieveResponse PBDNSRetrieveResponse
	response, err := a.Client.TryPostJSON(url, a.Pbauth)
	if err != nil { // failed response, set unspecified
		return netip.IPv4Unspecified(), err
	}
	defer response.Body.Close()

	if err = json.NewDecoder(response.Body).Decode(&dnsRetrieveResponse); err != nil {
		return netip.IPv4Unspecified(), err
	}

	// no record set
	if len(dnsRetrieveResponse.Records) < 1 {
		return netip.IPv4Unspecified(), nil
	}

	// take first result if multiple; each record *should* only correspond to
	// a single record on Porkbun's end
	rcd := dnsRetrieveResponse.Records[0]

	content := rcd.Content
	contentIP, err := netip.ParseAddr(content)
	if err != nil {
		return netip.IPv4Unspecified(), nil
	}
	return contentIP, nil
}

type AAAARecord struct {
	DnsRecord GenericDNSRecord
	Pbauth    *PBAuth
	Client    httpclient.IHttpClient
}

// creates a new AAAA record struct with an unspecified IP address answer
func NewAAAARecord(domain string, host string, ttl uint, auth *PBAuth, client httpclient.IHttpClient) (IPBRecord, error) {
	record := AAAARecord{
		DnsRecord: GenericDNSRecord{
			Domain: domain,
			Host:   host,
			Answer: netip.IPv6Unspecified(),
			Ttl:    ttl,
		},
		Pbauth: auth,
		Client: client,
	}

	currentAnswer, err := record.getCurrentAnswer()
	if err == nil {
		record.DnsRecord.Answer = currentAnswer
	}

	return &record, nil
}

// fetches the current answer for a given IP record.
func (a *AAAARecord) getCurrentAnswer() (netip.Addr, error) {
	// fetch existing record information
	url := fmt.Sprintf(dnsRecordFetchURL, a.DnsRecord.Domain, "AAAA", a.DnsRecord.Host)
	var dnsRetrieveResponse PBDNSRetrieveResponse
	response, err := a.Client.TryPostJSON(url, a.Pbauth)
	if err != nil { // failed response, set unspecified
		return netip.IPv6Unspecified(), err
	}
	defer response.Body.Close()

	if err = json.NewDecoder(response.Body).Decode(&dnsRetrieveResponse); err != nil {
		return netip.IPv6Unspecified(), err
	}

	// no record set
	if len(dnsRetrieveResponse.Records) < 1 {
		return netip.IPv6Unspecified(), nil
	}

	// take first result if multiple; each record *should* only correspond to
	// a single record on Porkbun's end
	rcd := dnsRetrieveResponse.Records[0]

	content := rcd.Content
	contentIP, err := netip.ParseAddr(content)
	if err != nil {
		return netip.IPv6Unspecified(), nil
	}
	return contentIP, nil
}
