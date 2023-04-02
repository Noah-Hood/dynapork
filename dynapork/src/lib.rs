#[cfg(feature = "docker")]
pub mod docker_secrets {
    use std::fs::File;
    use std::io::Read;

    const SECRET_PATH: &'static str = "run/secrets";

    pub enum SecretError {
        FileNotFound,
        FileReadError,
    }

    /// Try to read a string from a file
    fn try_get_string_from_file(file: &mut File) -> Result<String, SecretError> {
        let mut file_contents = String::new();
        let read_result = file.read_to_string(&mut file_contents);
        match read_result {
            Ok(_) => Ok(file_contents.trim().to_string()),
            Err(_) => Err(SecretError::FileReadError),
        }
    }

    /// Get a secret from the docker secrets directory by name
    pub fn try_get_secret_by_name(secret_name: &str) -> Result<String, SecretError> {
        let secret_file = File::open(format!("{SECRET_PATH}/{secret_name}"));

        match secret_file {
            Ok(mut file) => try_get_string_from_file(&mut file),
            Err(_) => Err(SecretError::FileNotFound),
        }
    }
}

pub mod config {
    use serde::{Deserialize, Serialize};
    use std::env;

    #[derive(Debug, Serialize, Deserialize)]
    pub struct Credentials {
        #[serde(rename = "apikey")]
        pub api_key: String,
        #[serde(rename = "secretapikey")]
        pub api_secret: String,
    }

    #[derive(Debug)]
    pub struct Config {
        pub credentials: Credentials,
        pub domain: String,
        pub subdomain: Option<String>,
    }

    #[derive(Debug)]
    pub enum ConfigError {
        ApiKeyNotFound,
        ApiSecretNotFound,
        DomainNotFound,
    }

    #[cfg(feature = "dev")]
    pub fn read_config() -> Result<Config, ConfigError> {
        // load dotenv
        dotenv::dotenv().ok();

        // get api key
        let api_key = env::var("APIKEY").map_err(|_| ConfigError::ApiKeyNotFound)?;

        // get api secret
        let api_secret = env::var("SECRETKEY").map_err(|_| ConfigError::ApiSecretNotFound)?;

        let credentials = Credentials {
            api_key,
            api_secret,
        };

        // get domain
        let domain = env::var("DOMAINNAME").map_err(|_| ConfigError::DomainNotFound)?;

        // get subdomain
        let subdomain = env::var("SUBDOMAIN").ok();

        Ok(Config {
            credentials,
            domain,
            subdomain,
        })
    }

    #[cfg(feature = "docker")]
    pub fn read_config() -> Result<Config, ConfigError> {
        // get api key
        let api_key = docker_secrets::try_get_secret_by_name("API_KEY")
            .map_err(|_| ConfigError::ApiKeyNotFound)?;

        // get api secret
        let api_secret = docker_secrets::try_get_secret_by_name("API_SECRET")
            .map_err(|_| ConfigError::ApiSecretNotFound)?;

        // get domain
        let domain = env::var("DOMAIN").map_err(|_| ConfigError::DomainNotFound)?;

        // get subdomain
        let subdomain = env::var("SUBDOMAIN").ok();

        Ok(Config {
            api_key,
            api_secret,
            domain,
            subdomain,
        })
    }
}
