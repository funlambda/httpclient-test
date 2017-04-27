﻿open System.Net.Http
open Http
open System.Net

let test1 client = async {
    let call = call client

    let! page = 
        call "http://www.cnn.com" GET
        |~> getStringBody

    return page
}

[<EntryPoint>]
let main argv = 
    let handler = new HttpClientHandler()
    handler.CookieContainer <- new CookieContainer()
    let client = new HttpClient(handler)

    let setCookie domain key value =
        let cookie = new Cookie(key, value)
        cookie.Domain <- domain
        handler.CookieContainer.Add(cookie)

    let res = test1 client |> Async.RunSynchronously
    printfn "%A" res
    0 // return an integer exit code
