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

func TestARecordNew(t *testing.T) {
	// logger := log.New(io.Discard, "", log.LstdFlags)

	t.Run("hits the API to request the initial value; sets unspecified when not extant", func(t *testing.T) {
		// arrange
		pbReturnValue := porkbunrecord.PBDNSRetrieveResponse{
			Status:     "SUCCESS",
			Cloudflare: "enabled",
			Records:    []porkbunrecord.PBDNSRecordResponse{},
		}

		client := MockHttpClient{
			returnValue: pbReturnValue,
		}

		auth := porkbunrecord.PBAuth{
			Secretapikey: "sk",
			ApiKey:       "ak",
		}

		expectedResponse := porkbunrecord.ARecord{
			DnsRecord: porkbunrecord.GenericDNSRecord{
				Domain: "domain.ext",
				Host:   "www",
				Answer: netip.IPv4Unspecified(),
				Ttl:    600,
			},
			Pbauth: &auth,
			Client: &client,
		}

		// act
		actualResponse, err := porkbunrecord.NewARecord("domain.ext", "www", 600, &auth, &client)

		// expect actualResponse to be of type porkbun.ARecord
		actualARecordResponse, ok := actualResponse.(*porkbunrecord.ARecord)
		if !ok {
			t.Fatalf("expected %v to be of type porkbunrecord.ARecord", actualARecordResponse)
		}

		if err != nil {
			t.Fatalf("failed to create A record: %v", err)
		}

		if !reflect.DeepEqual(&expectedResponse, actualResponse) {
			t.Fatalf("expected %v to equal %v", &expectedResponse, actualARecordResponse)
		}
	})

	t.Run("hits the API to request the initial value; sets value when extant", func(t *testing.T) {
		// arrange
		pbReturnValue := porkbunrecord.PBDNSRetrieveResponse{
			Status:     "SUCCESS",
			Cloudflare: "enabled",
			Records: []porkbunrecord.PBDNSRecordResponse{
				{
					Id:      "0123456789",
					Name:    "www.domain.ext",
					Type:    "A",
					Content: "66.65.34.128",
					Ttl:     "600",
					Prio:    "",
					Notes:   "",
				},
			},
		}

		client := MockHttpClient{
			returnValue: pbReturnValue,
		}

		auth := porkbunrecord.PBAuth{
			Secretapikey: "sk",
			ApiKey:       "ak",
		}

		expectedResponse := porkbunrecord.ARecord{
			DnsRecord: porkbunrecord.GenericDNSRecord{
				Domain: "domain.ext",
				Host:   "www",
				Answer: netip.AddrFrom4([4]byte{66, 65, 34, 128}),
				Ttl:    600,
			},
			Pbauth: &auth,
			Client: &client,
		}

		// act
		actualResponse, err := porkbunrecord.NewARecord("domain.ext", "www", 600, &auth, &client)

		// expect actualResponse to be of type porkbun.ARecord
		actualARecordResponse, ok := actualResponse.(*porkbunrecord.ARecord)
		if !ok {
			t.Fatalf("expected %v to be of type porkbunrecord.ARecord", actualARecordResponse)
		}

		if err != nil {
			t.Fatalf("failed to create A record: %v", err)
		}

		if !reflect.DeepEqual(&expectedResponse, actualResponse) {
			t.Fatalf("expected %v to equal %v", &expectedResponse, actualARecordResponse)
		}
	})
}

func TestAAAARecordNew(t *testing.T) {
	// logger := log.New(io.Discard, "", log.LstdFlags)

	t.Run("hits the API to request the initial value; sets unspecified when not extant", func(t *testing.T) {
		// arrange
		pbReturnValue := porkbunrecord.PBDNSRetrieveResponse{
			Status:     "SUCCESS",
			Cloudflare: "enabled",
			Records:    []porkbunrecord.PBDNSRecordResponse{},
		}

		client := MockHttpClient{
			returnValue: pbReturnValue,
		}

		auth := porkbunrecord.PBAuth{
			Secretapikey: "sk",
			ApiKey:       "ak",
		}

		expectedResponse := porkbunrecord.AAAARecord{
			DnsRecord: porkbunrecord.GenericDNSRecord{
				Domain: "domain.ext",
				Host:   "www",
				Answer: netip.IPv6Unspecified(),
				Ttl:    600,
			},
			Pbauth: &auth,
			Client: &client,
		}

		// act
		actualResponse, err := porkbunrecord.NewAAAARecord("domain.ext", "www", 600, &auth, &client)

		// expect actualResponse to be of type porkbun.ARecord
		actualAAAARecordResponse, ok := actualResponse.(*porkbunrecord.AAAARecord)
		if !ok {
			t.Fatalf("expected %v to be of type porkbunrecord.ARecord", actualAAAARecordResponse)
		}

		if err != nil {
			t.Fatalf("failed to create A record: %v", err)
		}

		if !reflect.DeepEqual(&expectedResponse, actualResponse) {
			t.Fatalf("expected %v to equal %v", &expectedResponse, actualAAAARecordResponse)
		}
	})

	t.Run("hits the API to request the initial value; sets value when extant", func(t *testing.T) {
		// arrange
		pbReturnValue := porkbunrecord.PBDNSRetrieveResponse{
			Status:     "SUCCESS",
			Cloudflare: "enabled",
			Records: []porkbunrecord.PBDNSRecordResponse{
				{
					Id:      "0123456789",
					Name:    "www.domain.ext",
					Type:    "AAAA",
					Content: "172a:a61e:8e1b:f0ee:6942:e41f:7f66:17c5",
					Ttl:     "600",
					Prio:    "",
					Notes:   "",
				},
			},
		}

		client := MockHttpClient{
			returnValue: pbReturnValue,
		}

		auth := porkbunrecord.PBAuth{
			Secretapikey: "sk",
			ApiKey:       "ak",
		}

		expectedResponse := porkbunrecord.AAAARecord{
			DnsRecord: porkbunrecord.GenericDNSRecord{
				Domain: "domain.ext",
				Host:   "www",
				Answer: netip.MustParseAddr("172a:a61e:8e1b:f0ee:6942:e41f:7f66:17c5"),
				Ttl:    600,
			},
			Pbauth: &auth,
			Client: &client,
		}

		// act
		actualResponse, err := porkbunrecord.NewAAAARecord("domain.ext", "www", 600, &auth, &client)

		// expect actualResponse to be of type porkbun.ARecord
		actualAAAARecordResponse, ok := actualResponse.(*porkbunrecord.AAAARecord)
		if !ok {
			t.Fatalf("expected %v to be of type porkbunrecord.AAAARecord", actualAAAARecordResponse)
		}

		if err != nil {
			t.Fatalf("failed to create AAAA record: %v", err)
		}

		if !reflect.DeepEqual(&expectedResponse, actualResponse) {
			t.Fatalf("expected %v to equal %v", &expectedResponse, actualAAAARecordResponse)
		}
	})
}
