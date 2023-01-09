I first reach out to an IP provider, specifically `icanhazip.com`,
and see what my public IP address is.

I compare that against the IP address of my domain with the Porkbun API.

If the two match, I don't do anything and check back in after a certain period 
of time.

If the two don't match, I update the Porkbun DNS record to point to my new
public IP.

I also keep track of the length of my DNS Record TTL; when it is nearly expired I will reach out
and refresh it.
