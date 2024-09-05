package porkbunrecord_test

import (
	"bytes"
	"encoding/json"
	"io"
	"net/http"
	"net/netip"
	"reflect"
	"testing"
	"time"

	"github.com/hoodnoah/dynapork/internal/ipmonitor"
	"github.com/hoodnoah/dynapork/internal/porkbunrecord"
)

type HttpCall struct {
	Url  string
	Body interface{}
}

type MockHttpClient struct {
	Calls                  []HttpCall
	FetchStringReturnValue string
	PostJsonReturnValue    interface{}
	ErrorReturnValue       error
}

func (m *MockHttpClient) TryFetchString(url string) (string, error) {
	m.Calls = append(m.Calls, HttpCall{Url: url, Body: nil})

	if m.ErrorReturnValue != nil {
		return "", m.ErrorReturnValue
	}

	return m.FetchStringReturnValue, nil
}

func (m *MockHttpClient) TryPostJSON(url string, body interface{}) (*http.Response, error) {
	m.Calls = append(m.Calls, HttpCall{Url: url, Body: body})

	if m.ErrorReturnValue != nil {
		return nil, m.ErrorReturnValue
	}

	jsonValue, err := json.Marshal(m.PostJsonReturnValue)
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

type MockIPMonitor struct {
	Subv4Channel chan ipmonitor.IpChange
	Subv6Channel chan ipmonitor.IpChange
}

func (m *MockIPMonitor) SubscribeV4() <-chan ipmonitor.IpChange {
	return m.Subv4Channel
}

func (m *MockIPMonitor) SubscribeV6() <-chan ipmonitor.IpChange {
	return m.Subv6Channel
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

		client := MockHttpClient{PostJsonReturnValue: pbReturnValue}
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

		client := MockHttpClient{PostJsonReturnValue: pbReturnValue}
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

		client := MockHttpClient{PostJsonReturnValue: pbReturnValue}
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

		client := MockHttpClient{PostJsonReturnValue: pbReturnValue}
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

func TestSubscribe(t *testing.T) {
	t.Run("it should receive the correct channel based on its record type; v4", func(t *testing.T) {
		// arrange
		// setup mock IP monitor
		v4Chan := make(chan ipmonitor.IpChange, 1)
		v6Chan := make(chan ipmonitor.IpChange, 1)

		monitor := MockIPMonitor{
			Subv4Channel: v4Chan,
			Subv6Channel: v6Chan,
		}

		// create DNS record directly (obviate mocking a client)
		rcd := porkbunrecord.DNSRecord{
			Domain:     "noah-hood.io",
			Subdomain:  "sdm",
			RecordType: porkbunrecord.A,
			Answer:     netip.MustParseAddr("66.65.64.63"),
			Ttl:        600,
			Client:     nil,
			Auth:       &porkbunrecord.PBAuth{},
		}

		// act
		rcd.Subscribe(&monitor)

		// assert
		if v4Chan != rcd.UpdateChannel {
			t.Fatalf("expected %v to equal %v", v4Chan, rcd.UpdateChannel)
		}
	})

	t.Run("it should receive the correct channel based on its record type; v6", func(t *testing.T) {
		// arrange
		// setup mock IP monitor
		v4Chan := make(chan ipmonitor.IpChange, 1)
		v6Chan := make(chan ipmonitor.IpChange, 1)

		monitor := MockIPMonitor{
			Subv4Channel: v4Chan,
			Subv6Channel: v6Chan,
		}

		// create DNS record directly (obviate mocking a client)
		rcd := porkbunrecord.DNSRecord{
			Domain:     "noah-hood.io",
			Subdomain:  "sdm",
			RecordType: porkbunrecord.AAAA,
			Answer:     netip.MustParseAddr("6893:8cce:26dc:5d13:5055:fe71:3f47:8e5a"),
			Ttl:        600,
			Client:     nil,
			Auth:       &porkbunrecord.PBAuth{},
		}

		// act
		rcd.Subscribe(&monitor)

		// assert
		if v6Chan != rcd.UpdateChannel {
			t.Fatalf("expected %v to equal %v", v4Chan, rcd.UpdateChannel)
		}
	})

	t.Run("it should hit the API when it is provided with a change", func(t *testing.T) {
		// arrange
		// setup mock IP monitor
		v4Chan := make(chan ipmonitor.IpChange, 1)
		v6Chan := make(chan ipmonitor.IpChange, 1)

		monitor := MockIPMonitor{
			Subv4Channel: v4Chan,
			Subv6Channel: v6Chan,
		}

		// setup mock client
		mockClient := MockHttpClient{
			PostJsonReturnValue: porkbunrecord.PBDNSEditSuccessResponse{},
		}

		// create DNS record directly (obviate mocking a client)
		rcd := porkbunrecord.DNSRecord{
			Domain:     "noah-hood.io",
			Subdomain:  "sdm",
			RecordType: porkbunrecord.A,
			Answer:     netip.MustParseAddr("66.65.64.63"),
			Ttl:        600,
			Client:     &mockClient,
			Auth: &porkbunrecord.PBAuth{
				Secretapikey: "sk",
				ApiKey:       "ak",
			},
		}

		expectedUrl := "https://api.porkbun.com/api/json/v3/dns/editByNameType/noah-hood.io/A/sdm"

		// act
		rcd.Subscribe(&monitor)

		// assert
		// should be no calls on subscribe alone
		if len(mockClient.Calls) != 0 {
			t.Fatalf("expected no calls to be made, received %d", len(mockClient.Calls))
		}

		// push an update to the channel
		v4Chan <- ipmonitor.IpChange{
			From: netip.MustParseAddr("66.65.64.63"),
			To:   netip.MustParseAddr("66.65.64.62"),
		}

		// wait briefly given asynchrony
		time.Sleep(500 * time.Millisecond)

		// assert that the call was made (and only the one call)
		if len(mockClient.Calls) != 1 {
			t.Fatalf("expected a single call, received %d", len(mockClient.Calls))
		}

		actualUrl := mockClient.Calls[0].Url

		if expectedUrl != actualUrl {
			t.Fatalf("expected actual URL %s to equal %s", actualUrl, expectedUrl)
		}
	})
}
