package porkbunrecord

// porkbun request/response types
type PBAuth struct {
	Secretapikey string `json:"secretapikey"`
	ApiKey       string `json:"apikey"`
}

type PBFailResponse struct {
	Status  string `json:"status"`
	Message string `json:"message"`
}

type PBDNSRecordResponse struct {
	Id      string `json:"id"`
	Name    string `json:"name"`
	Type    string `json:"type"`
	Content string `json:"content"`
	Ttl     string `json:"ttl"`
	Prio    string `json:"prio"`
	Notes   string `json:"notes"`
}

type PBDNSRetrieveResponse struct {
	Status     string                `json:"status"`
	Cloudflare string                `json:"cloudflare"`
	Records    []PBDNSRecordResponse `json:"records"`
}
