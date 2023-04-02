use dynapork::config;

pub mod request_ip {
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

    #[derive(Serialize, Deserialize)]
    struct SuccessResponse {
        status: String,
        yourIp: String,
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
        let url = "https://api-ipv4.porkbun.com/api/json/v3/ping";
        let result = client.post(url).json(&credentials).send()?;
        let response = result.json::<PingResponse>()?;

        match response {
            PingResponse::SuccessResponse(response) => Ok(response.yourIp),
            PingResponse::FailureResponse(response) => {
                eprintln!("Error: {}", response.message);
                std::process::exit(1)
            }
        }
    }
}

fn main() {
    let reqwest_client = reqwest::blocking::Client::new();
    // step 1: load configuration
    let config = match config::read_config() {
        Ok(config) => config,
        Err(err) => {
            eprintln!("Error loading configuration: {:?}", err);
            std::process::exit(1)
        }
    };

    println!("{:?}", config);

    // step 2: request ip address
    let ip = request_ip::request_ip(&reqwest_client, config.credentials);

    println!("{:?}", ip);
}
