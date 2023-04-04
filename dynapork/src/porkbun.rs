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
    SuccessResponse(SuccessResponse),
    FailureResponse(FailureResponse),
}

pub fn request_ip<T: HttpClient>(
    client: &T,
    credentials: config::Credentials,
) -> Result<String, reqwest::Error> {
    let result = client.post_json(PING_URL, &credentials)?;
    let response = result.json::<PingResponse>()?;

    match response {
        PingResponse::SuccessResponse(response) => Ok(response.your_ip),
        PingResponse::FailureResponse(response) => {
            eprintln!("Error: {}", response.message);
            std::process::exit(1)
        }
    }
}
