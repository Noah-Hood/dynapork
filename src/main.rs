use dynapork::config;

fn main() {
    let config = config::try_read_config().unwrap();

    println!("{:?}", config);
}
