use dynapork::config;

pub mod request_ip {
    use super::config;
    use reqwest;
    pub fn request_ip(credentials: config::Credentials) -> Result<String, reqwest::Error> {
        let client = reqwest::blocking::Client::new();
        let url = "https://api.ipify.org";
        let response = client.get(url).send()?;
        let ip = response.text()?;
        Ok(ip)
    }
}

fn main() {
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
}
