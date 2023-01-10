## IP Monitoring

I reach out periodically to a website to retrieve my public IP Address.

Then I compare it to my last known IP address, which can be either blank (the first time I've checked)
or it can contain an old IP address.

If the new one is not the same as the old one, I take down the new one in place of the old one and tell everyone
that the address has changed.

## DNS Record Maintenance

My most common job is to keep track of the TTL of the DNS record.

When the record is about to expire, I reach back out to PorkBun and refresh the TTL with the old DNS record information from
before. When I refresh the TTL, I let everyone know I've refreshed it, when, and for how long.

I also keep an eye out for when the IP address has changed.

If I notice it's changed, I gather the new IP address value and reach out to PorkBun to update the listing. I refresh the TTL
at the same time, and I tell everyone I have updated the DNS listing.
