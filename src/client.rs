use reqwest::blocking::Client;
use serde::Serialize;
use std::error::Error;

pub trait HttpClient {
    fn post_json<T: Serialize>(&self, url: &str, body: T) -> Result<String, Box<dyn Error>>;
}

impl HttpClient for Client {
    fn post_json<T: Serialize>(&self, url: &str, body: T) -> Result<String, Box<dyn Error>> {
        let response = self.post(url).json(&body).send()?;
        let response_text = response.text()?;
        Ok(response_text)
    }
}
