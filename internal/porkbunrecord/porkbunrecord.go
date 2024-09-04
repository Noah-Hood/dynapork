package porkbunrecord

import (
	"encoding/json"
	"fmt"
	"net/http"
	"net/netip"

	"github.com/hoodnoah/dynapork/internal/httpclient"
)

const dnsRecordFetchURL = "https://api.porkbun.com/api/json/v3/dns/retrieveByNameType/%s/%s/%s" // url, type, subdomain

type RecordType uint

const (
	A RecordType = iota
	AAAA
)

type DNSRecord struct {
	Domain     string
	Subdomain  string
	RecordType RecordType
	Answer     netip.Addr
	Ttl        uint
	Client     httpclient.IHttpClient
	Auth       *PBAuth
}

type APIError struct {
	code    int
	message string
}

func (a *APIError) Error() string {
	return fmt.Sprintf("porkbun API request failed with status %d: %s", a.code, a.message)
}

type NoRecordError struct {
	Domain     string
	Subdomain  string
	Recordtype string
}

func (n *NoRecordError) Error() string {
	return fmt.Sprintf("no record exists for the provided domain, subdomain, and type: %s, %s, %s", n.Domain, n.Subdomain, n.Recordtype)
}

type AmbiguousRecordError struct {
	Domain     string
	Subdomain  string
	Recordtype string
}

func (a *AmbiguousRecordError) Error() string {
	return fmt.Sprintf("more than one record exists for the provided domain, subdomain, and type: %s, %s, %s", a.Domain, a.Subdomain, a.Recordtype)
}

func genericUnspecified(rt RecordType) netip.Addr {
	switch rt {
	case A:
		return netip.IPv4Unspecified()
	case AAAA:
		return netip.IPv6Unspecified()
	default:
		return netip.Addr{}
	}
}

// Constructor for a new DNS record.
// Fetches existing DNS information to populate Answer field; if no answer can be found, returns an error.
// This is because the DNSRecord can *only* update existing records, not create new ones.
func NewDNSRecord(domain string, subdomain string, recordType RecordType, ttl uint, auth *PBAuth, client httpclient.IHttpClient) (DNSRecord, error) {
	record := DNSRecord{
		Domain:     domain,
		Subdomain:  subdomain,
		RecordType: recordType,
		Answer:     genericUnspecified(recordType),
		Ttl:        ttl,
		Client:     client,
		Auth:       auth,
	}

	ip, err := record.getCurrentContent()
	if err != nil {
		return DNSRecord{}, err
	}

	record.Answer = ip

	return record, nil
}

// Gets the current content i.e. the current IP Address pointed to by a given record.
// Fails when there is no record set, or when there is more than one record set.
// On failure, returns an unspecified IP address in line with the type of record.
// e.g. an A record returns an unspecified IPV4, AAAA returns unspecified IPV6.
func (d *DNSRecord) getCurrentContent() (netip.Addr, error) {
	// submit web request
	url := constructRetrieveUrl(d)

	res, err := d.Client.TryPostJSON(url, d.Auth)
	if err != nil {
		return genericUnspecified(d.RecordType), nil
	}

	// handle response
	switch res.StatusCode {
	// request successful, read results and pass along parsed IP address value
	case http.StatusOK:
		// parse result
		var retrieveResult PBDNSRetrieveResponse
		if err := json.NewDecoder(res.Body).Decode(&retrieveResult); err != nil {
			return genericUnspecified(d.RecordType), err
		}
		// if there's no record in the API, return an error; do not create a record
		if len(retrieveResult.Records) < 1 {
			return genericUnspecified(d.RecordType), &NoRecordError{
				Domain:     d.Domain,
				Subdomain:  d.Subdomain,
				Recordtype: recordTypeToString(d.RecordType),
			}
		}

		// if there is more than one record, return an error
		// there should only ever be 1 dns record for a given endpoint
		// managed by this client
		if len(retrieveResult.Records) > 1 {
			return genericUnspecified(d.RecordType), &AmbiguousRecordError{
				Domain:     d.Domain,
				Subdomain:  d.Subdomain,
				Recordtype: recordTypeToString(d.RecordType),
			}
		}

		// try parsing first and only result into an ip address
		ip, err := netip.ParseAddr(retrieveResult.Records[0].Content)
		if err != nil {
			return genericUnspecified(d.RecordType), err
		}

		// return ip on success
		return ip, nil // likely should validate that the ip address type matches the expected type, e.g. v4 vs v6

	// handle any failure; the message is within the returned JSON, not the header
	default:
		return genericUnspecified(d.RecordType), handleFailureResult(res)
	}
}

// Handles an API failure.
// Parses the failure JSON, if possible, to extract a meaningful error message.
func handleFailureResult(response *http.Response) error {
	var failureResult PBFailResponse
	if err := json.NewDecoder(response.Body).Decode(&failureResult); err != nil {
		return err
	}
	return &APIError{
		code:    response.StatusCode,
		message: failureResult.Message,
	}
}

// forms the parts of the Retrieval URL based on the constituent members of a DNSRecord
func constructRetrieveUrl(d *DNSRecord) string {
	var typeString string
	switch d.RecordType {
	case A:
		typeString = "A"
	case AAAA:
		typeString = "AAAA"
	default:
		typeString = ""
	}

	return fmt.Sprintf(dnsRecordFetchURL, d.Domain, typeString, d.Subdomain)
}

// transforms a recordType enum into a string
func recordTypeToString(recordType RecordType) string {
	switch recordType {
	case A:
		return "A"
	case AAAA:
		return "AAAA"
	default:
		return ""
	}
}
