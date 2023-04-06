use serde::{Deserialize, Serialize};
use std::error::Error;

use crate::client::HttpClient;
use crate::constants::PING_URL;

#[derive(Serialize, Deserialize, Debug, PartialEq)]
pub struct PingSuccessResponse {
    status: String,
    your_ip: String,
}

#[derive(Serialize, Deserialize, Debug)]
pub struct Credentials {
    api_key: String,
    secret_key: String,
}

pub fn ping(
    client: &impl HttpClient<ResponseBody = String>,
    credentials: &Credentials,
) -> Result<String, Box<dyn Error>> {
    let response_body = client.post_json(PING_URL, credentials)?;

    let response: PingSuccessResponse = serde_json::from_str(&response_body)?;
    Ok(response.your_ip)
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

    impl HttpClient for MockHttpClient<String> {
        type ResponseBody = String;

        fn post_json<T: Serialize>(
            &self,
            _url: &str,
            _body: T,
        ) -> Result<Self::ResponseBody, Box<dyn Error>> {
            Ok(serde_json::to_string(&self.response_body)?)
        }
    }

    impl HttpClient for MockHttpClient<PingSuccessResponse> {
        type ResponseBody = String;

        fn post_json<T: Serialize>(
            &self,
            _url: &str,
            _body: T,
        ) -> Result<Self::ResponseBody, Box<dyn Error>> {
            Ok(serde_json::to_string(&self.response_body)?)
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
}
