package main

import (
	"fmt"

	"github.com/hoodnoah/dynapork/internal/httpclient"
)

func main() {
	client := httpclient.RateLimitedHttpClient_new(1)

	url := "https://www.icanhazip.com"

	result, err := client.TryFetchString(url)

	if err != nil {
		fmt.Printf("failed to fetch %s: %v\n", url, err.Error())
	}

	fmt.Printf("Received: %s\n", result)
}
