#[cfg(test)]
pub struct MockHttpClient<T: serde::Serialize> {
    pub response_body: T,
}
