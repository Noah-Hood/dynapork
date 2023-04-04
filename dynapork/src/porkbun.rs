// local lib
use super::config;

// external crates
use reqwest;
use serde::{Deserialize, Serialize};

// traits
pub trait HttpClient {
    fn post_json<T: Serialize>(
        &self,
        url: &str,
        body: &T,
    ) -> Result<reqwest::blocking::Response, reqwest::Error>;
}

// implement HttpClient trait for reqwest Client
impl HttpClient for reqwest::blocking::Client {
    fn post_json<T: Serialize>(
        self: &reqwest::blocking::Client,
        url: &str,
        body: &T,
    ) -> Result<reqwest::blocking::Response, reqwest::Error> {
        self.post(url).json(body).send()
    }
}

#[derive(Serialize, Deserialize)]
struct FailureResponse {
    status: String,
    message: String,
}

// constants
const PING_URL: &'static str = "https://api-ipv4.porkbun.com/api/json/v3/ping";

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

pub fn request_ip<T: HttpClient>(
    client: &T,
    credentials: config::Credentials,
) -> Result<String, reqwest::Error> {
    let result = client.post_json(PING_URL, &credentials)?;
    let response = result.json::<PingResponse>()?;

    match response {
        PingResponse::Success(response) => Ok(response.your_ip),
        PingResponse::Failure(response) => {
            eprintln!("Error: {}", response.message);
            std::process::exit(1)
        }
    }
}

#[cfg(test)]
mod request_ip_tests {
    use crate::config::Credentials;

    use super::{request_ip, HttpClient, PingResponse, SuccessResponse};

    struct MockHttpClient {
        response: PingResponse,
    }

    impl HttpClient for MockHttpClient {
        fn post_json<T: serde::Serialize>(
            self: &MockHttpClient,
            _url: &str,
            _body: &T,
        ) -> Result<reqwest::blocking::Response, reqwest::Error> {
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
        let result = request_ip(&mock_client, credentials).unwrap_or("failure".to_owned());

        assert_eq!(result, expected_ip);
    }
}
