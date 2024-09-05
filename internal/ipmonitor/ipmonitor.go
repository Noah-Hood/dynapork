package ipmonitor

import (
	"errors"
	"log"
	"net/netip"
	"sync"
	"time"

	"github.com/hoodnoah/dynapork/internal/httpclient"
)

const v4Url = "https://api.ipify.org"
const v6Url = "https://api6.ipify.org"
const maxRetries = 10

// custom error type for retries exceeded
type RetriesExceededError struct{}

func (e *RetriesExceededError) Error() string {
	return "maxRetries exceeded"
}

type IPV4 netip.Addr
type IPV6 netip.Addr

type IpChange struct {
	From netip.Addr
	To   netip.Addr
}

type IIpMonitor interface {
	SubscribeV4() <-chan IpChange
	SubscribeV6() <-chan IpChange
}

type IpMonitor struct {
	client        httpclient.IHttpClient
	subscribersv4 []chan IpChange
	subscribersv6 []chan IpChange
	currentIpV4   IPV4
	currentIpV6   IPV6
	mutex         sync.Mutex
	logger        *log.Logger
	interval      time.Duration
}

// ctor
func NewIpMonitor(client httpclient.IHttpClient, logger *log.Logger, interval time.Duration) IIpMonitor {
	monitor := IpMonitor{
		client:        client,
		subscribersv4: []chan IpChange{},
		subscribersv6: []chan IpChange{},
		currentIpV4:   IPV4{},
		currentIpV6:   IPV6{},
		mutex:         sync.Mutex{},
		logger:        logger,
		interval:      interval,
	}

	go monitor.start() // start monitoring in new goroutine

	return &monitor
}

// receive a read-only channel which will be populated with
// detected IPv4 address changes
func (i *IpMonitor) SubscribeV4() <-chan IpChange {
	i.mutex.Lock()
	defer i.mutex.Unlock()

	updateChannel := make(chan IpChange, 1)
	i.subscribersv4 = append(i.subscribersv4, updateChannel)

	return updateChannel
}

// receive a read-only channel which will be populated with
// detected IPv6 address changes
func (i *IpMonitor) SubscribeV6() <-chan IpChange {
	i.mutex.Lock()
	defer i.mutex.Unlock()

	updateChannel := make(chan IpChange, 1)
	i.subscribersv6 = append(i.subscribersv6, updateChannel)

	return updateChannel
}

// starts the monitoring function
func (i *IpMonitor) start() {
	for {
		// check for ipv4 change
		v4, err := fetchV4(i.client)
		if err != nil {
			i.logger.Printf("[ERROR]: failed to fetch ipv4: %v\n", err)
		}
		if i.currentIpV4 != v4 {
			change := IpChange{
				From: netip.Addr(i.currentIpV4),
				To:   netip.Addr(v4),
			}
			// publish
			i.publish(change, i.subscribersv4)

			// set new ip
			i.currentIpV4 = v4

			i.logger.Printf("[INFO]: %s -> %s", change.From.String(), change.To.String())
		}

		// check for ipv6 change
		v6, err := fetchV6(i.client)
		if err != nil {
			i.logger.Printf("[ERROR]: failed to fetch ipv6: %v\n", err)
		}
		if i.currentIpV6 != v6 {
			change := IpChange{
				From: netip.Addr(i.currentIpV6),
				To:   netip.Addr(v6),
			}
			// publish
			i.publish(change, i.subscribersv6)

			// set new ip
			i.currentIpV6 = v6

			i.logger.Printf("[INFO]: %s -> %s", change.From.String(), change.To.String())
		}

		time.Sleep(i.interval)
	}
}

// publishes an ip change to all provided subscribers
func (i *IpMonitor) publish(change IpChange, subscribers []chan IpChange) {
	for _, subscriber := range subscribers {
		subscriber <- change
	}
}

// **** HELPERS ****
// helper function to fetch an ip address
// retries when rate limit is exceeded
func fetchIp(client httpclient.IHttpClient, endpoint string) (netip.Addr, error) {
	tries := 0

	for tries < maxRetries {
		ipString, err := client.TryFetchString(endpoint)
		if err != nil {
			if errors.Is(err, &httpclient.RateLimitedError{}) {
				time.Sleep(500 * time.Millisecond)
			} else {
				return netip.IPv4Unspecified(), err
			}
		}
		tries++

		parsedIp, err := netip.ParseAddr(ipString)
		if err != nil {
			return netip.IPv4Unspecified(), err
		} else {
			return parsedIp, nil
		}
	}

	return netip.IPv4Unspecified(), &RetriesExceededError{}
}

func fetchV4(client httpclient.IHttpClient) (IPV4, error) {
	ip, err := fetchIp(client, v4Url)
	if err != nil {
		return IPV4(netip.IPv4Unspecified()), err
	}

	return IPV4(ip), nil
}

func fetchV6(client httpclient.IHttpClient) (IPV6, error) {
	ip, err := fetchIp(client, v6Url)
	if err != nil {
		return IPV6(netip.IPv6Unspecified()), err
	}

	return IPV6(ip), nil
}
