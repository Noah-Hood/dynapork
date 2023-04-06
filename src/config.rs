use std::env;
use std::error::Error;

use crate::constants::{
    API_KEY_VAR_NAME, DOMAIN_NAME_VAR_NAME, RECORD_TYPE_VAR_NAME, SECRET_KEY_VAR_NAME,
    SUBDOMAIN_VAR_NAME,
};

#[derive(Debug, PartialEq)]
pub enum RecordType {
    A,
    AAAA,
    CNAME,
    MX,
    TXT,
    SRV,
    NS,
    PTR,
    CAA,
}

fn to_record_type(record_type_string: &str) -> Result<RecordType, Box<dyn Error>> {
    match record_type_string {
        "A" => Ok(RecordType::A),
        "AAAA" => Ok(RecordType::AAAA),
        "CNAME" => Ok(RecordType::CNAME),
        "MX" => Ok(RecordType::MX),
        "TXT" => Ok(RecordType::TXT),
        "SRV" => Ok(RecordType::SRV),
        "NS" => Ok(RecordType::NS),
        "PTR" => Ok(RecordType::PTR),
        "CAA" => Ok(RecordType::CAA),
        _ => Err("Invalid record type".into()),
    }
}

#[derive(Debug, PartialEq)]
pub struct Config {
    pub api_key: String,
    pub secret_key: String,
    pub domain_name: String,
    pub subdomain: Option<String>,
    pub record_type: RecordType,
}

// get environment variables
#[cfg(feature = "dev")]
pub fn try_read_config() -> Result<Config, Box<dyn Error>> {
    // load .env file
    dotenv::dotenv().ok();

    let api_key = env::var(API_KEY_VAR_NAME)?;
    let secret_key = env::var(SECRET_KEY_VAR_NAME)?;
    let domain_name = env::var(DOMAIN_NAME_VAR_NAME)?;
    let subdomain = env::var(SUBDOMAIN_VAR_NAME).ok();
    let record_type = env::var(RECORD_TYPE_VAR_NAME).unwrap_or("A".to_string());
    let record_type = to_record_type(&record_type.to_uppercase())?;

    Ok(Config {
        api_key,
        secret_key,
        domain_name,
        subdomain,
        record_type,
    })
}
