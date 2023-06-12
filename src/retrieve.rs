/// Handler for retrieval of DNS record information by domain name
use crate::client::HttpClient;
use crate::common_types::{Credentials, RecordType};
use crate::constants::RETRIEVE_URL;
use serde::{Deserialize, Deserializer, Serialize};
use serde_json;
use std::error::Error;
use std::str::FromStr;

fn int_str_decoder<'de, D, T>(deserializer: D) -> Result<T, D::Error>
where
    D: Deserializer<'de>,
    T: FromStr,
    T::Err: std::fmt::Display,
{
    let s = String::deserialize(deserializer)?;
    T::from_str(&s).map_err(serde::de::Error::custom)
}

fn int_from_optional_str_decoder<'de, D, T>(deserializer: D) -> Result<Option<T>, D::Error>
where
    D: Deserializer<'de>,
    T: FromStr,
    T::Err: std::fmt::Display,
{
    let opt: Option<String> = Option::deserialize(deserializer)?;
    match opt {
        Some(s) => match T::from_str(&s) {
            Ok(v) => Ok(Some(v)),
            Err(_) => Err(serde::de::Error::custom(
                "Failed to parse optional string to i32",
            )),
        },
        None => Ok(None),
    }
}

#[derive(Serialize, Deserialize, Debug)]
pub struct RetrieveRecord {
    #[serde(deserialize_with = "int_str_decoder")]
    pub id: i32,
    pub name: String,
    #[serde(rename = "type")]
    pub record_type: RecordType,
    pub content: String,
    #[serde(deserialize_with = "int_str_decoder")]
    pub ttl: i32,
    #[serde(deserialize_with = "int_from_optional_str_decoder")]
    pub prio: Option<i32>,
    pub notes: Option<String>,
}

#[derive(Serialize, Deserialize, Debug)]
pub enum CloudFlareStatus {
    #[serde(rename = "enabled")]
    Enabled,
    #[serde(rename = "disabled")]
    Disabled,
}

#[derive(Serialize, Deserialize, Debug)]
pub struct RetrieveSuccessResult {
    pub status: String,
    #[serde(rename = "cloudflare")]
    pub cloudflare_status: CloudFlareStatus,
    pub records: Vec<RetrieveRecord>,
}

pub fn retrieve_records_by_domain(
    client: &impl HttpClient<ResponseBody = String>,
    credentials: &Credentials,
    domain: &str,
) -> Result<RetrieveSuccessResult, Box<dyn Error>> {
    let domain_url = format!("{}/{}", RETRIEVE_URL, domain);

    let response_body = client.post_json(&domain_url, credentials)?;

    let parsed_response = serde_json::from_str::<RetrieveSuccessResult>(&response_body)?;

    Ok(parsed_response)
}
