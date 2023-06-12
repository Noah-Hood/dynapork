// use dynapork::client;
use dynapork::common_types::Credentials;
use dynapork::config;
// use dynapork::ping::ping;
use dynapork::retrieve::retrieve_records_by_domain;
use reqwest::blocking;

fn main() {
    let client = blocking::Client::new();

    let config = config::try_read_config().unwrap();

    let credentials = Credentials {
        api_key: config.api_key,
        secret_key: config.secret_key,
    };

    let domain = "noah-hood.io";

    let result = retrieve_records_by_domain(&client, &credentials, &domain);

    match result {
        Ok(d) => println!("{:?}", d),
        Err(e) => println!("Error: {}", e),
    }
}
