package httpclient

import (
	"bytes"
	"encoding/json"
	"io"
	"net/http"
	"time"
)

// custom error type for rate limit
type RateLimitedError struct{}

func (e *RateLimitedError) Error() string {
	return "rate limit exceeded"
}

type IHttpClient interface {
	TryFetchString(url string) (string, error)
	TryPostJSON(url string, jsonInput interface{}) (*http.Response, error)
}

type RateLimitedHttpClient struct {
	client    http.Client
	limiter   *time.Ticker
	tokenPool chan bool
}

// construct a new IHttpClient instance
func NewRateLimitedClient(requestsPerSecond int) IHttpClient {
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

	default: // rate limit exceeded
		return "", &RateLimitedError{}
	}
}

// attempts to post a json payload to a provided url,
// returns the response directly for later processing
func (r RateLimitedHttpClient) TryPostJSON(url string, jsonInput interface{}) (*http.Response, error) {
	select {
	case <-r.tokenPool: // token available
		requestData, err := json.Marshal(jsonInput)
		if err != nil {
			return nil, err
		}

		request, err := http.NewRequest("POST", url, bytes.NewBuffer(requestData))
		if err != nil {
			return nil, err
		}

		response, err := r.client.Do(request)
		if err != nil {
			return nil, err
		}

		return response, nil

	default: // rate limit exceeded
		return nil, &RateLimitedError{}
	}
}
