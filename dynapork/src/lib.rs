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
    pub fn get_secret(secret_name: &str) -> Result<String, SecretError> {
        let secret_file = File::open(format!("{SECRET_PATH}/{secret_name}"));

        match secret_file {
            Ok(mut file) => try_get_string_from_file(&mut file),
            Err(_) => Err(SecretError::FileNotFound),
        }
    }
}
