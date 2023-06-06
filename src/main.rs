// use dynapork::client;
use dynapork::config;
use dynapork::ping::{ping, Credentials};
use reqwest::blocking;

fn main() {
    let client = blocking::Client::new();

    let config = config::try_read_config().unwrap();

    let credentials = Credentials {
        api_key: config.api_key,
        secret_key: config.secret_key,
    };

    let result = ping(&client, &credentials);

    match result {
        Ok(ip) => println!("Your IP address is: {}", ip),
        Err(e) => println!("Error: {}", e),
    }
}
