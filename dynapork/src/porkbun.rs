// local lib
use super::config;

// external crates
use reqwest;
use serde::{Deserialize, Serialize};
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

pub fn request_ip(
    client: &reqwest::blocking::Client,
    credentials: config::Credentials,
) -> Result<String, reqwest::Error> {
    let result = client.post(PING_URL).json(&credentials).send()?;
    let response = result.json::<PingResponse>()?;

    match response {
        PingResponse::SuccessResponse(response) => Ok(response.your_ip),
        PingResponse::FailureResponse(response) => {
            eprintln!("Error: {}", response.message);
            std::process::exit(1)
        }
    }
}
