use serde::{Deserialize, Serialize};

#[derive(Serialize, Deserialize, Debug)]
pub struct Credentials {
    #[serde(rename = "apikey")]
    pub api_key: String,
    #[serde(rename = "secretapikey")]
    pub secret_key: String,
}

#[derive(Serialize, Deserialize, Debug)]
pub enum RecordType {
    A,
    MX,
    CNAME,
    ALIAS,
    TXT,
    NS,
    AAAA,
    SRV,
    TLSA,
    CAA,
}
