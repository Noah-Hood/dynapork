use dynapork::client;
use dynapork::client::HttpClient;
use dynapork::config;
use reqwest::blocking;

fn main() {
    let client = blocking::Client::new();

    let result = client
        .post_json("https://httpbin.org/post", "Hello, world!")
        .unwrap();

    println!("{}", result);
}
