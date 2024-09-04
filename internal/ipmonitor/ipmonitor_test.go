package ipmonitor_test

import (
	"io"
	"log"
	"net/http"
	"net/netip"
	"strings"
	"testing"
	"time"

	"github.com/hoodnoah/dynapork/internal/ipmonitor"
)

type MockHttpClient struct {
	V4Result      string
	V6Result      string
	ReturnError   bool
	ErrorToReturn error
}

func (m *MockHttpClient) TryFetchString(url string) (string, error) {
	if m.ReturnError {
		return "", m.ErrorToReturn
	}
	if strings.Contains(url, "6") {
		return m.V6Result, nil
	} else {
		return m.V4Result, nil
	}
}

func (m *MockHttpClient) TryPostJSON(_ string, _ interface{}) (*http.Response, error) {
	return nil, nil
}

func TestIpMonitorV4(t *testing.T) {
	logger := log.New(io.Discard, "", log.LstdFlags)

	t.Run("pushes an initial change to a single v4 subscriber", func(t *testing.T) {
		// arrange
		expectedResult := ipmonitor.IpChange{
			From: netip.Addr{}, // expect to start with blank/empty addr
			To:   netip.AddrFrom4([4]byte{192, 168, 1, 1}),
		}

		client := MockHttpClient{
			V4Result:      "192.168.1.1",
			V6Result:      "0c71:d460:0961:07de:62d5:0f87:ac61:2f38",
			ReturnError:   false,
			ErrorToReturn: nil,
		}

		monitor := ipmonitor.NewIpMonitor(&client, logger, 50*time.Millisecond)
		subscriber := monitor.SubscribeV4()

		// delay
		time.Sleep(250 * time.Millisecond)

		result := <-subscriber

		if expectedResult != result {
			t.Fatalf("Expected %v to equal %v", expectedResult, result)
		}
	})

	t.Run("pushes an initial change to multiple v4 subscribers", func(t *testing.T) {
		// arrange
		expectedResult := ipmonitor.IpChange{
			From: netip.Addr{}, // expect to start with blank/empty addr
			To:   netip.AddrFrom4([4]byte{192, 168, 1, 1}),
		}

		client := MockHttpClient{
			V4Result:      "192.168.1.1",
			V6Result:      "0c71:d460:0961:07de:62d5:0f87:ac61:2f38",
			ReturnError:   false,
			ErrorToReturn: nil,
		}

		monitor := ipmonitor.NewIpMonitor(&client, logger, 50*time.Millisecond)
		subscriber := monitor.SubscribeV4()
		subscriber2 := monitor.SubscribeV4()

		// delay
		time.Sleep(250 * time.Millisecond)

		result := <-subscriber
		result2 := <-subscriber2

		if (expectedResult != result) || (expectedResult != result2) {
			t.Fatalf("Expected %v to equal %v", expectedResult, result)
		}
	})

	t.Run("pushes subsequent updates to all v4 subscribers", func(t *testing.T) {
		// arrange
		expectedResult := ipmonitor.IpChange{
			From: netip.AddrFrom4([4]byte{192, 168, 1, 1}),
			To:   netip.AddrFrom4([4]byte{192, 168, 1, 2}),
		}

		client := MockHttpClient{
			V4Result:      "192.168.1.1",
			V6Result:      "0c71:d460:0961:07de:62d5:0f87:ac61:2f38",
			ReturnError:   false,
			ErrorToReturn: nil,
		}

		monitor := ipmonitor.NewIpMonitor(&client, logger, 50*time.Millisecond)
		subscriber := monitor.SubscribeV4()
		subscriber2 := monitor.SubscribeV4()

		// delay
		time.Sleep(250 * time.Millisecond)

		// consume first result, discard
		<-subscriber
		<-subscriber2

		client.V4Result = "192.168.1.2"
		time.Sleep(250 * time.Millisecond)

		// consume second result
		result := <-subscriber
		result2 := <-subscriber2

		if (expectedResult != result) || (expectedResult != result2) {
			t.Fatalf("Expected %v to equal %v", expectedResult, result)
		}
	})

	t.Run("pushes no updates to v4 subscribers when there's no change", func(t *testing.T) {
		client := MockHttpClient{
			V4Result:      "192.168.1.1",
			V6Result:      "0c71:d460:0961:07de:62d5:0f87:ac61:2f38",
			ReturnError:   false,
			ErrorToReturn: nil,
		}

		monitor := ipmonitor.NewIpMonitor(&client, logger, 50*time.Millisecond)
		subscriber := monitor.SubscribeV4()

		// delay
		time.Sleep(250 * time.Millisecond)

		// consume first result, discard
		<-subscriber

		client.V4Result = "192.168.1.1"
		time.Sleep(250 * time.Millisecond)

		// consume second result
		select {
		case val := <-subscriber:
			t.Fatalf("expected channel to contain nothing, received value %v", val)
		default:
			// no value rec'd, expected case
		}
	})

	t.Run("pushes no updates to v4 subscribers when there's no *v4* change", func(t *testing.T) {
		client := MockHttpClient{
			V4Result:      "192.168.1.1",
			V6Result:      "0c71:d460:0961:07de:62d5:0f87:ac61:2f38",
			ReturnError:   false,
			ErrorToReturn: nil,
		}

		monitor := ipmonitor.NewIpMonitor(&client, logger, 50*time.Millisecond)
		subscriber := monitor.SubscribeV4()

		// delay
		time.Sleep(250 * time.Millisecond)

		// consume first result, discard
		<-subscriber

		client.V6Result = "0c71:d460:0961:07de:62d5:0f87:ac61:2f39"
		time.Sleep(250 * time.Millisecond)

		// consume second result
		select {
		case val := <-subscriber:
			t.Fatalf("expected channel to contain nothing, received value %v", val)
		default:
			// no value rec'd, expected case
		}
	})
}

func TestIpMonitorV6(t *testing.T) {
	logger := log.New(io.Discard, "", log.LstdFlags)

	t.Run("pushes an initial change to a single v6 subscriber", func(t *testing.T) {
		// arrange
		expectedResult := ipmonitor.IpChange{
			From: netip.Addr{}, // expect to start with blank/empty addr
			To:   netip.MustParseAddr("0c71:d460:0961:07de:62d5:0f87:ac61:2f38"),
		}

		client := MockHttpClient{
			V4Result:      "192.168.1.1",
			V6Result:      "0c71:d460:0961:07de:62d5:0f87:ac61:2f38",
			ReturnError:   false,
			ErrorToReturn: nil,
		}

		monitor := ipmonitor.NewIpMonitor(&client, logger, 50*time.Millisecond)
		subscriber := monitor.SubscribeV6()

		// delay
		time.Sleep(250 * time.Millisecond)

		result := <-subscriber

		if expectedResult != result {
			t.Fatalf("Expected %v to equal %v", expectedResult, result)
		}
	})

	t.Run("pushes an initial change to multiple v6 subscribers", func(t *testing.T) {
		// arrange
		expectedResult := ipmonitor.IpChange{
			From: netip.Addr{}, // expect to start with blank/empty addr
			To:   netip.MustParseAddr("0c71:d460:0961:07de:62d5:0f87:ac61:2f38"),
		}

		client := MockHttpClient{
			V4Result:      "192.168.1.1",
			V6Result:      "0c71:d460:0961:07de:62d5:0f87:ac61:2f38",
			ReturnError:   false,
			ErrorToReturn: nil,
		}

		monitor := ipmonitor.NewIpMonitor(&client, logger, 50*time.Millisecond)
		subscriber := monitor.SubscribeV6()
		subscriber2 := monitor.SubscribeV6()

		// delay
		time.Sleep(250 * time.Millisecond)

		result := <-subscriber
		result2 := <-subscriber2

		if (expectedResult != result) || (expectedResult != result2) {
			t.Fatalf("Expected %v to equal %v", expectedResult, result)
		}
	})

	t.Run("pushes subsequent updates to all v6 subscribers", func(t *testing.T) {
		// arrange
		expectedResult := ipmonitor.IpChange{
			From: netip.MustParseAddr("0c71:d460:0961:07de:62d5:0f87:ac61:2f38"),
			To:   netip.MustParseAddr("0c71:d460:0961:07de:62d5:0f87:ac61:2f39"),
		}

		client := MockHttpClient{
			V4Result:      "192.168.1.1",
			V6Result:      "0c71:d460:0961:07de:62d5:0f87:ac61:2f38",
			ReturnError:   false,
			ErrorToReturn: nil,
		}

		monitor := ipmonitor.NewIpMonitor(&client, logger, 50*time.Millisecond)
		subscriber := monitor.SubscribeV6()
		subscriber2 := monitor.SubscribeV6()

		// delay
		time.Sleep(250 * time.Millisecond)

		// consume first result, discard
		<-subscriber
		<-subscriber2

		client.V6Result = "0c71:d460:0961:07de:62d5:0f87:ac61:2f39"
		time.Sleep(250 * time.Millisecond)

		// consume second result
		result := <-subscriber
		result2 := <-subscriber2

		if (expectedResult != result) || (expectedResult != result2) {
			t.Fatalf("Expected %v to equal %v", expectedResult, result)
		}
	})

	t.Run("pushes no updates to v6 subscribers when there's no change", func(t *testing.T) {
		client := MockHttpClient{
			V4Result:      "192.168.1.1",
			V6Result:      "0c71:d460:0961:07de:62d5:0f87:ac61:2f38",
			ReturnError:   false,
			ErrorToReturn: nil,
		}

		monitor := ipmonitor.NewIpMonitor(&client, logger, 50*time.Millisecond)
		subscriber := monitor.SubscribeV6()

		// delay
		time.Sleep(250 * time.Millisecond)

		// consume first result, discard
		<-subscriber

		client.V4Result = "0c71:d460:0961:07de:62d5:0f87:ac61:2f38"
		time.Sleep(250 * time.Millisecond)

		// consume second result
		select {
		case val := <-subscriber:
			t.Fatalf("expected channel to contain nothing, received value %v", val)
		default:
			// no value rec'd, expected case
		}
	})

	t.Run("pushes no updates to v6 subscribers when there's no *v6* change", func(t *testing.T) {
		client := MockHttpClient{
			V4Result:      "192.168.1.1",
			V6Result:      "0c71:d460:0961:07de:62d5:0f87:ac61:2f38",
			ReturnError:   false,
			ErrorToReturn: nil,
		}

		monitor := ipmonitor.NewIpMonitor(&client, logger, 50*time.Millisecond)
		subscriber := monitor.SubscribeV6()

		// delay
		time.Sleep(250 * time.Millisecond)

		// consume first result, discard
		<-subscriber

		client.V4Result = "192.168.1.2"
		time.Sleep(250 * time.Millisecond)

		// consume second result
		select {
		case val := <-subscriber:
			t.Fatalf("expected channel to contain nothing, received value %v", val)
		default:
			// no value rec'd, expected case
		}
	})
}
