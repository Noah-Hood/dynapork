// local lib
use super::config;

// external crates
use reqwest;
use serde::{Deserialize, Serialize};

// purkbun API response types
#[derive(Serialize, Deserialize)]
struct FailureResponse {
    status: String,
    message: String,
}

#[derive(Serialize, Deserialize)]
struct SuccessResponse {
    status: String,
    #[serde(rename = "yourIp")]
    your_ip: String,
}

#[derive(Serialize, Deserialize)]
#[serde(untagged)]
enum PingResponse {
    Success(SuccessResponse),
    Failure(FailureResponse),
}

// custom result types
#[derive(Debug, PartialEq)]
pub enum PorkbunError {
    InvalidCredentialsError,
    APIError(String),
    ResponseDecodeError,
    WebRequestError,
}

// traits
pub trait HttpClient {
    fn post_json<T: Serialize>(
        &self,
        url: &str,
        body: &T,
    ) -> Result<reqwest::blocking::Response, PorkbunError>;
}

// implement HttpClient trait for reqwest Client
impl HttpClient for reqwest::blocking::Client {
    fn post_json<T: Serialize>(
        self: &reqwest::blocking::Client,
        url: &str,
        body: &T,
    ) -> Result<reqwest::blocking::Response, PorkbunError> {
        let result = self.post(url).json(body).send();
        match result {
            Ok(response) => Ok(response),
            Err(_) => Err(PorkbunError::WebRequestError),
        }
    }
}

// constants
const PING_URL: &'static str = "https://api-ipv4.porkbun.com/api/json/v3/ping";

/// Request the current IP address from Porkbun
/// # Arguments
/// * `client` - A reference to an object that implements the HttpClient trait
/// * `credentials` - A Credentials struct containing the API key and secret
/// # Returns
/// * `Ok(String)` - The current IP address
/// * `Err(PorkbunError)` - An error occurred
pub fn request_ip<T: HttpClient>(
    client: &T,
    credentials: &config::Credentials,
) -> Result<String, PorkbunError> {
    let result = client.post_json(PING_URL, &credentials)?; // returns a PorkbunError, can propagate
    let json_contents = result.json::<PingResponse>();
    match json_contents {
        Err(_) => Err(PorkbunError::ResponseDecodeError),
        Ok(PingResponse::Success(response)) => Ok(response.your_ip),
        Ok(PingResponse::Failure(response)) => match response.message.as_str() {
            "Invalid API key. (002)" => Err(PorkbunError::InvalidCredentialsError),
            _ => Err(PorkbunError::APIError(response.message)),
        },
    }
}

#[cfg(test)]
mod request_ip_tests {
    use crate::config::Credentials;

    use super::{request_ip, HttpClient, PingResponse, PorkbunError, SuccessResponse};

    struct MockHttpClient {
        response: PingResponse,
    }

    impl HttpClient for MockHttpClient {
        fn post_json<T: serde::Serialize>(
            self: &MockHttpClient,
            _url: &str,
            _body: &T,
        ) -> Result<reqwest::blocking::Response, PorkbunError> {
            let response_string =
                serde_json::to_string(&self.response).unwrap_or("Could not serialize".to_owned());

            let reqwest_response =
                reqwest::blocking::Response::from(http::response::Response::new(response_string));
            Ok(reqwest_response)
        }
    }

    #[test]
    fn returns_expected_ip_on_success() {
        let credentials = Credentials {
            api_key: "".to_owned(),
            api_secret: "".to_owned(),
        };
        let expected_ip = String::from("192.168.1.1");
        let expected_response = PingResponse::Success(SuccessResponse {
            status: "SUCCESS".to_owned(),
            your_ip: expected_ip.clone(),
        });
        let mock_client = MockHttpClient {
            response: expected_response,
        };
        let result = request_ip(&mock_client, &credentials).unwrap_or("failure".to_owned());

        assert_eq!(result, expected_ip);
    }

    #[test]
    fn returns_credential_error_on_credential_failure() {
        let credentials = Credentials {
            api_key: "".to_owned(),
            api_secret: "".to_owned(),
        };
        let expected_result = PorkbunError::InvalidCredentialsError;

        let mock_client = MockHttpClient {
            response: PingResponse::Failure(super::FailureResponse {
                status: "ERROR".to_owned(),
                message: "Invalid API key. (002)".to_owned(),
            }),
        };

        let result = request_ip(&mock_client, &credentials).unwrap_err();

        assert_eq!(expected_result, result);
    }

    #[test]
    fn returns_unspecified_error_on_generic_failure() {
        let credentials = Credentials {
            api_key: "".to_owned(),
            api_secret: "".to_owned(),
        };
        let expected_result =
            PorkbunError::APIError("Non-specific, unknown error (000)".to_owned());

        let mock_client = MockHttpClient {
            response: PingResponse::Failure(super::FailureResponse {
                status: "ERROR".to_owned(),
                message: "Non-specific, unknown error (000)".to_owned(),
            }),
        };

        let result = request_ip(&mock_client, &credentials).unwrap_err();

        assert_eq!(expected_result, result);
    }
}
