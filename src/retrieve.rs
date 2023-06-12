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

#[derive(Serialize, Deserialize, Debug, PartialEq)]
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

#[derive(Serialize, Deserialize, Debug, PartialEq)]
pub enum CloudFlareStatus {
    #[serde(rename = "enabled")]
    Enabled,
    #[serde(rename = "disabled")]
    Disabled,
}

#[derive(Serialize, Deserialize, Debug, PartialEq)]
pub struct RetrieveSuccessResponse {
    pub status: String,
    #[serde(rename = "cloudflare")]
    pub cloudflare_status: CloudFlareStatus,
    pub records: Vec<RetrieveRecord>,
}

#[derive(Serialize, Deserialize, Debug, PartialEq)]
pub struct RetrieveErrorResponse {
    pub status: String,
    pub message: String,
}

#[derive(Serialize, Deserialize, Debug, PartialEq)]
pub enum RetrieveError {
    InvalidCredentials,
    MissingAPIKey,
    InvalidAPIKey,
    InvalidSecretKey,
    InvalidDomain,
    InvalidPermissions,
    Unknown,
}

impl std::error::Error for RetrieveError {}

impl std::fmt::Display for RetrieveError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            RetrieveError::InvalidCredentials => write!(f, "Invalid credentials"),
            RetrieveError::MissingAPIKey => write!(f, "Missing API key"),
            RetrieveError::InvalidAPIKey => write!(f, "Invalid API key"),
            RetrieveError::InvalidSecretKey => write!(f, "Invalid secret key"),
            RetrieveError::InvalidDomain => write!(f, "Invalid domain"),
            RetrieveError::InvalidPermissions => write!(f, "Invalid permissions"),
            RetrieveError::Unknown => write!(f, "Unknown error"),
        }
    }
}

#[derive(Serialize, Deserialize, Debug)]
#[serde(untagged)]
pub enum RetrieveResponse {
    Success(RetrieveSuccessResponse),
    Failure(RetrieveErrorResponse),
}

pub fn retrieve_records_by_domain(
    client: &impl HttpClient<ResponseBody = String>,
    credentials: &Credentials,
    domain: &str,
) -> Result<RetrieveSuccessResponse, Box<dyn Error>> {
    let domain_url = format!("{}/{}", RETRIEVE_URL, domain);

    let response_body = client.post_json(&domain_url, credentials)?;

    let parsed_response = serde_json::from_str::<RetrieveResponse>(&response_body)?;

    match parsed_response {
        RetrieveResponse::Success(success) => Ok(success),
        RetrieveResponse::Failure(failure) => match failure.message.as_str() {
            "All API requests must provide minimal required data." => {
                Err(Box::new(RetrieveError::InvalidCredentials))
            }
            "All API requests require an API key." => Err(Box::new(RetrieveError::MissingAPIKey)),
            "Invalid API key. (001)" => Err(Box::new(RetrieveError::InvalidAPIKey)),
            "Invalid API key. (002)" => Err(Box::new(RetrieveError::InvalidSecretKey)),
            "Invalid domain." => Err(Box::new(RetrieveError::InvalidDomain)),
            "Domain is not opted in to API access." => {
                Err(Box::new(RetrieveError::InvalidPermissions))
            }
            _ => Err(Box::new(RetrieveError::Unknown)),
        },
    }
}

#[cfg(test)]
mod retrieve_tests {
    use super::*;
    use crate::client::HttpClient;
    use crate::common_types::Credentials;
    use crate::test_fixtures::MockHttpClient;
    use serde::Serialize;
    use std::error::Error;

    impl HttpClient for MockHttpClient<String> {
        type ResponseBody = String;

        fn post_json<U: Serialize>(
            &self,
            _url: &str,
            _body: U,
        ) -> Result<Self::ResponseBody, Box<dyn Error>> {
            Ok(self.response_body.clone())
        }
    }

    #[test]
    fn returns_correct_result_on_success() {
        let default_credentials = Credentials {
            api_key: "AK".to_string(),
            secret_key: "SK".to_string(),
        };

        let client = MockHttpClient {
            response_body: SUCCESS_RESPONSE.to_string(),
        };

        let expected_result = RetrieveSuccessResponse {
            status: "SUCCESS".to_string(),
            cloudflare_status: CloudFlareStatus::Enabled,
            records: vec![
                RetrieveRecord {
                    id: 1234567,
                    name: "pfx.website.ext".to_string(),
                    record_type: RecordType::A,
                    content: "192.168.0.0".to_string(),
                    ttl: 600,
                    prio: Some(0),
                    notes: None,
                },
                RetrieveRecord {
                    id: 1234568,
                    name: "website.ext".to_string(),
                    record_type: RecordType::NS,
                    content: "maceio.porkbun.com".to_string(),
                    ttl: 86400,
                    prio: None,
                    notes: None,
                },
                RetrieveRecord {
                    id: 1234569,
                    name: "website.ext".to_string(),
                    record_type: RecordType::NS,
                    content: "salvador.porkbun.com".to_string(),
                    ttl: 86400,
                    prio: None,
                    notes: None,
                },
                RetrieveRecord {
                    id: 1234570,
                    name: "website.ext".to_string(),
                    record_type: RecordType::NS,
                    content: "fortaleza.porkbun.com".to_string(),
                    ttl: 86400,
                    prio: None,
                    notes: None,
                },
                RetrieveRecord {
                    id: 1234571,
                    name: "website.ext".to_string(),
                    record_type: RecordType::NS,
                    content: "curitiba.porkbun.com".to_string(),
                    ttl: 86400,
                    prio: None,
                    notes: None,
                },
                RetrieveRecord {
                    id: 1234572,
                    name: "_acme-challenge.website.ext".to_string(),
                    record_type: RecordType::TXT,
                    content: "random_string".to_string(),
                    ttl: 600,
                    prio: None,
                    notes: None,
                },
                RetrieveRecord {
                    id: 1234573,
                    name: "_acme-challenge.website.ext".to_string(),
                    record_type: RecordType::TXT,
                    content: "random_string".to_string(),
                    ttl: 600,
                    prio: None,
                    notes: None,
                },
            ],
        };

        let result =
            retrieve_records_by_domain(&client, &default_credentials, DEFAULT_DOMAIN).unwrap();

        assert_eq!(result, expected_result);
    }

    const DEFAULT_DOMAIN: &str = "pfx.website.ext";

    const SUCCESS_RESPONSE: &str = r#"
{
  "status": "SUCCESS",
  "cloudflare": "enabled",
  "records": [
    {
      "id": "1234567",
      "name": "pfx.website.ext",
      "type": "A",
      "content": "192.168.0.0",
      "ttl": "600",
      "prio": "0",
      "notes": null
    },
    {
      "id": "1234568",
      "name": "website.ext",
      "type": "NS",
      "content": "maceio.porkbun.com",
      "ttl": "86400",
      "prio": null,
      "notes": null
    },
    {
      "id": "1234569",
      "name": "website.ext",
      "type": "NS",
      "content": "salvador.porkbun.com",
      "ttl": "86400",
      "prio": null,
      "notes": null
    },
    {
      "id": "1234570",
      "name": "website.ext",
      "type": "NS",
      "content": "fortaleza.porkbun.com",
      "ttl": "86400",
      "prio": null,
      "notes": null
    },
    {
      "id": "1234571",
      "name": "website.ext",
      "type": "NS",
      "content": "curitiba.porkbun.com",
      "ttl": "86400",
      "prio": null,
      "notes": null
    },
    {
      "id": "1234572",
      "name": "_acme-challenge.website.ext",
      "type": "TXT",
      "content": "random_string",
      "ttl": "600",
      "prio": null,
      "notes": null
    },
    {
      "id": "1234573",
      "name": "_acme-challenge.website.ext",
      "type": "TXT",
      "content": "random_string",
      "ttl": "600",
      "prio": null,
      "notes": null
    }
  ]
}"#;

    const MISSING_CREDENTIALS_RESPONSE: &str = r#"
{
    "status": "FAILURE",
    "message": "All API requests must provide minimal required data."
}
"#;

    const MISSING_API_KEY_RESPONSE: &str = r#"
{
    "status": "FAILURE",
    "message": "All API requests require an API key."
}  
"#;

    const INVALID_CREDENTIALS_RESPONSE_01: &str = r#"
{
    "status": "FAILURE",
    "message": "Invalid API key. (001)"
}
"#;

    const INVALID_CREDENTIALS_RESPONSE_02: &str = r#"
{
    "status: "FAILURE",
    "message": "Invalid API key. (002)"
}    
"#;

    const INVALID_DOMAIN_RESPONSE: &str = r#"
{
    "status": "FAILURE",
    "message": "Invalid domain."
}
"#;

    const INVALID_PERMISSIONS_RESPONSE: &str = r#"
{
    "status": "FAILURE",
    "message": "Domain is not opted in to API access."
}
"#;
}
