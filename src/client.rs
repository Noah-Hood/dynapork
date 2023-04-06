use reqwest::blocking::Client;
use serde::Serialize;
use std::error::Error;

pub trait HttpClient {
    type ResponseBody;

    fn post_json<T: Serialize>(
        &self,
        url: &str,
        body: T,
    ) -> Result<Self::ResponseBody, Box<dyn Error>>;
}

impl HttpClient for Client {
    type ResponseBody = String;
    fn post_json<T: Serialize>(&self, url: &str, body: T) -> Result<String, Box<dyn Error>> {
        let response = self.post(url).json(&body).send()?;
        let response_text = response.text()?;
        Ok(response_text)
    }
}
