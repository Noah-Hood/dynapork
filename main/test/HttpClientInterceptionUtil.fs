module HttpClientInterceptionUtil

open JustEat.HttpClientInterception
open Thoth.Json.Net
open Domain.Ping

/// <summary>Sets the return value of an
/// <code>HttpClientInterceptionBuilder</code>
/// registers it with the provided options,
/// and return it.</summary>
let setBuilderContent<'T>
    (ctntToStr: 'T -> string)
    (options: HttpClientInterceptorOptions)
    (bldr: HttpRequestInterceptionBuilder)
    (ctnt: 'T)
    =
    let strContent = ctntToStr ctnt

    bldr
        .Responds()
        .WithContent(strContent)
        .RegisterWith(options)

/// <summary>Sets the content for which the
/// <code>HttpRequestInterceptionBilder</code> will respond.</summary>
let setBuilderExpectedContent<'T> (encoder: 'T -> JsonValue) (bldr: HttpRequestInterceptionBuilder) (ctnt: 'T) =
    let strContent = ctnt |> encoder |> (fun x -> x.ToString())

    bldr.ForContent(
        (fun x ->
            (x.ReadAsStringAsync()
             |> Async.AwaitTask
             |> Async.RunSynchronously) = strContent)
    )

/// <summary>Creates a default <code>HttpRequestInterceptionBuilder<code>
/// which responts to HTTPS POST requests against <code>host</code>
/// for <code>path</code>
let createDefaultBuilder host path =
    HttpRequestInterceptionBuilder()
        .Requests()
        .ForHttps()
        .ForPost()
        .ForHost(host)
        .ForPath(path)
