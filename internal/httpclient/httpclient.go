package httpclient

import (
	"errors"
	"io"
	"net/http"
	"time"
)

type IHttpClient interface {
	TryFetchString(url string) (string, error)
}

type RateLimitedHttpClient struct {
	client    http.Client
	limiter   *time.Ticker
	tokenPool chan bool
}

// construct a new IHttpClient instance
func RateLimitedHttpClient_new(requestsPerSecond int) IHttpClient {
	limiter := time.NewTicker(time.Second / time.Duration(requestsPerSecond))
	tokenPool := make(chan bool, 2)
	tokenPool <- true // prime token pool at initialization to prevent dead time
	client := http.Client{}

	r := RateLimitedHttpClient{
		client,
		limiter,
		tokenPool,
	}

	go r.quenchPool() // start token pool

	return &r
}

// replenishes tokens at the interval defined by requestsPerSecond
func (r *RateLimitedHttpClient) quenchPool() {
	for range r.limiter.C {
		select {
		case r.tokenPool <- true:
		default: // discard token if full
		}
	}
}

// fetches the string at the provided URL.
// fails when the rate limit is exceeded
func (r RateLimitedHttpClient) TryFetchString(url string) (string, error) {
	select {
	case <-r.tokenPool:
		response, err := r.client.Get(url)
		if err != nil {
			return "", err
		}
		defer response.Body.Close()

		bodyBytes, err := io.ReadAll(response.Body)
		if err != nil {
			return "", err
		}

		return string(bodyBytes), nil

	default:
		return "", errors.New("rate limit exceeded")
	}
}
