use dynapork::config;
use dynapork::porkbun;

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
    let ip = porkbun::request_ip(&reqwest_client, &config.credentials);

    println!("{:?}", ip);
}
