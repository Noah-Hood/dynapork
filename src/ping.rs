use serde::{Deserialize, Serialize};
use std::error::Error;

use crate::client::HttpClient;
use crate::constants::PING_URL;

#[derive(Serialize, Deserialize, Debug, PartialEq)]
pub struct PingSuccessResponse {
    status: String,
    #[serde(rename = "yourIp")]
    your_ip: String,
}

#[derive(Serialize, Deserialize, Debug, PartialEq)]
pub struct PingFailureResponse {
    status: String,
    message: String,
}

#[derive(Serialize, Deserialize, Debug, PartialEq)]
#[serde(untagged)]
pub enum PingResponse {
    Success(PingSuccessResponse),
    Failure(PingFailureResponse),
}

#[derive(Deserialize, PartialEq, Debug)]
pub enum PingError {
    InvalidCredentials,
    Unknown,
}

impl std::fmt::Display for PingError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            PingError::InvalidCredentials => write!(f, "Invalid credentials"),
            PingError::Unknown => write!(f, "Unknown error"),
        }
    }
}

impl Error for PingError {}

#[derive(Serialize, Deserialize, Debug)]
pub struct Credentials {
    #[serde(rename = "apikey")]
    pub api_key: String,
    #[serde(rename = "secretapikey")]
    pub secret_key: String,
}

/// Pings the Porkbun API with the provided client, returning the IP Address
/// of the caller, if successful, and an error otherwise.
pub fn ping(
    client: &impl HttpClient<ResponseBody = String>,
    credentials: &Credentials,
) -> Result<String, Box<dyn Error>> {
    let response_body = client.post_json(PING_URL, credentials)?;

    let parsed_response = serde_json::from_str::<PingResponse>(&response_body)?;

    match parsed_response {
        PingResponse::Success(success) => Ok(success.your_ip),
        PingResponse::Failure(failure) => match failure.message.as_str() {
            "Invalid API key. (002)" => Err(Box::new(PingError::InvalidCredentials)),
            _ => Err(Box::new(PingError::Unknown)),
        },
    }
}

#[cfg(test)]
mod ping_tests {
    use super::*;
    use crate::client::HttpClient;
    use serde::Serialize;
    use std::error::Error;

    const IP_ADDRESS: &str = "192.168.1.1";

    struct MockHttpClient<T: Serialize> {
        response_body: T,
    }

    impl<T: Serialize> HttpClient for MockHttpClient<T> {
        type ResponseBody = String;

        fn post_json<U: Serialize>(
            &self,
            _url: &str,
            _body: U,
        ) -> Result<Self::ResponseBody, Box<dyn Error>> {
            let response_body = serde_json::to_string(&self.response_body)?;

            Ok(response_body)
        }
    }

    #[test]
    fn returns_ip_provided_by_api_on_success() {
        let default_credentials = Credentials {
            api_key: "AK".to_string(),
            secret_key: "SK".to_string(),
        };
        let client = MockHttpClient {
            response_body: PingSuccessResponse {
                status: "SUCCESS".to_string(),
                your_ip: IP_ADDRESS.to_string(),
            },
        };

        let result = super::ping(&client, &default_credentials).unwrap();

        assert_eq!(result, IP_ADDRESS);
    }

    #[test]
    fn returns_invalid_credentials_error_on_correct_failure() {
        let default_credentials = Credentials {
            api_key: "AK".to_string(),
            secret_key: "SK".to_string(),
        };
        let client = MockHttpClient {
            response_body: PingFailureResponse {
                status: "ERROR".to_string(),
                message: "Invalid API key. (002)".to_string(),
            },
        };

        let result = ping(&client, &default_credentials).unwrap_err();

        let expected_result = Box::new(super::PingError::InvalidCredentials);

        // downcast the dynamic Error to PingError and compare
        // will fail if cannot be downcast to PingError
        assert_eq!(result.downcast::<PingError>().unwrap(), expected_result);
    }
}
