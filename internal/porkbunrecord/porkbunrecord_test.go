package porkbunrecord_test

import (
	"bytes"
	"encoding/json"
	"io"
	"net/http"
	"net/netip"
	"reflect"
	"testing"

	"github.com/hoodnoah/dynapork/internal/porkbunrecord"
)

type MockHttpClient struct {
	returnValue porkbunrecord.PBDNSRetrieveResponse
}

func (m MockHttpClient) TryFetchString(_ string) (string, error) {
	return "", nil
}

func (m MockHttpClient) TryPostJSON(_ string, auth interface{}) (*http.Response, error) {
	jsonValue, err := json.Marshal(m.returnValue)
	if err != nil {
		return nil, err
	}

	buffer := bytes.NewBuffer(jsonValue)

	response := http.Response{}
	response.Status = "OK"
	response.StatusCode = 200
	response.Body = io.NopCloser(buffer)
	response.Header = make(http.Header)

	return &response, nil
}

func TestConstructor(t *testing.T) {
	dummyAuth := porkbunrecord.PBAuth{
		Secretapikey: "sk",
		ApiKey:       "ak",
	}

	t.Run("sets the current content when retrieved from the API (v4)", func(t *testing.T) {
		// arrange
		pbReturnValue := porkbunrecord.PBDNSRetrieveResponse{
			Status:     "SUCCESS",
			Cloudflare: "enabled",
			Records: []porkbunrecord.PBDNSRecordResponse{
				{
					Id:      "123456789",
					Name:    "sdm.noah-hood.io",
					Type:    "A",
					Content: "66.65.64.63",
					Ttl:     "600",
					Prio:    "",
					Notes:   "a record for sdm subdomain",
				},
			},
		}

		client := MockHttpClient{returnValue: pbReturnValue}
		expectedResponse := porkbunrecord.DNSRecord{
			Domain:     "noah-hood.io",
			Subdomain:  "sdm",
			RecordType: porkbunrecord.A,
			Answer:     netip.AddrFrom4([4]byte{66, 65, 64, 63}),
			Ttl:        600,
			Client:     &client,
			Auth:       &dummyAuth,
		}

		// act
		actualResponse, err := porkbunrecord.NewDNSRecord("noah-hood.io", "sdm", porkbunrecord.A, 600, &dummyAuth, &client)
		if err != nil {
			t.Fatalf("failed to create new DNS record")
		}

		if !reflect.DeepEqual(expectedResponse, actualResponse) {
			t.Fatalf("expected %v to equal %v", expectedResponse, actualResponse)
		}
	})

	t.Run("sets the current content when retrieved from the API (v6)", func(t *testing.T) {
		// arrange
		pbReturnValue := porkbunrecord.PBDNSRetrieveResponse{
			Status:     "SUCCESS",
			Cloudflare: "enabled",
			Records: []porkbunrecord.PBDNSRecordResponse{
				{
					Id:      "123456789",
					Name:    "sdm.noah-hood.io",
					Type:    "AAAA",
					Content: "839c:7b4c:9651:5a20:74e9:6165:2c5d:a126",
					Ttl:     "600",
					Prio:    "",
					Notes:   "a record for sdm subdomain",
				},
			},
		}

		client := MockHttpClient{returnValue: pbReturnValue}
		expectedResponse := porkbunrecord.DNSRecord{
			Domain:     "noah-hood.io",
			Subdomain:  "sdm",
			RecordType: porkbunrecord.AAAA,
			Answer:     netip.MustParseAddr("839c:7b4c:9651:5a20:74e9:6165:2c5d:a126"),
			Ttl:        600,
			Client:     &client,
			Auth:       &dummyAuth,
		}

		// act
		actualResponse, err := porkbunrecord.NewDNSRecord("noah-hood.io", "sdm", porkbunrecord.AAAA, 600, &dummyAuth, &client)
		if err != nil {
			t.Fatalf("failed to create new DNS record")
		}

		if !reflect.DeepEqual(expectedResponse, actualResponse) {
			t.Fatalf("expected %v to equal %v", expectedResponse, actualResponse)
		}
	})

	t.Run("errors out when there is no extant record on the API side", func(t *testing.T) {
		// arrange
		pbReturnValue := porkbunrecord.PBDNSRetrieveResponse{
			Status:     "SUCCESS",
			Cloudflare: "enabled",
			Records:    []porkbunrecord.PBDNSRecordResponse{},
		}

		client := MockHttpClient{returnValue: pbReturnValue}
		expectedResponse := &porkbunrecord.NoRecordError{
			Domain:     "noah-hood.io",
			Subdomain:  "sdm",
			Recordtype: "AAAA",
		}

		// act
		_, err := porkbunrecord.NewDNSRecord("noah-hood.io", "sdm", porkbunrecord.AAAA, 600, &dummyAuth, &client)

		if err == nil {
			t.Fatalf("expected to receive an error value, received nil")
		}

		if !reflect.DeepEqual(expectedResponse, err) {
			t.Fatalf("expected to receive %v, received %v", expectedResponse, err)
		}
	})

	t.Run("errors out when there is more than one record for a single DNSRecord", func(t *testing.T) {
		// arrange
		pbReturnValue := porkbunrecord.PBDNSRetrieveResponse{
			Status:     "SUCCESS",
			Cloudflare: "enabled",
			Records: []porkbunrecord.PBDNSRecordResponse{
				{
					Id:      "123456789",
					Name:    "sdm.noah-hood.io",
					Type:    "A",
					Content: "66.65.64.63",
					Ttl:     "600",
					Prio:    "",
					Notes:   "a record for sdm subdomain",
				},
				{
					Id:      "123456790",
					Name:    "sdm.noah-hood.io",
					Type:    "A",
					Content: "66.65.64.62",
					Ttl:     "600",
					Prio:    "",
					Notes:   "a second record for sdm subdomain",
				},
			},
		}

		client := MockHttpClient{returnValue: pbReturnValue}
		expectedResponse := &porkbunrecord.AmbiguousRecordError{
			Domain:     "noah-hood.io",
			Subdomain:  "sdm",
			Recordtype: "A",
		}
		// act
		_, err := porkbunrecord.NewDNSRecord("noah-hood.io", "sdm", porkbunrecord.A, 600, &dummyAuth, &client)
		if err == nil {
			t.Fatalf("expected to receive an error, received nil")
		}

		if !reflect.DeepEqual(expectedResponse, err) {
			t.Fatalf("expected %v to equal %v", expectedResponse, err)
		}
	})
}
