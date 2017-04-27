module Http

open System.Net.Http
open System.IO
open System.Net

//let serialize, deserialize = Serialization.defaultSerializer

//let deserializet<'a> bytes =
//    deserialize (typeof<'a>, "", bytes) :?> 'a

let (|~>) w1 f = 
    async {
        let! x = w1
        return! f x
    }

let (|@>) a1 f = 
    async { 
        let! x = a1
        return f x 
    }

let test (f: 'a -> bool) msg (x: 'a) = 
    if not (f x) then failwith msg

let (|!>) a f =
    async { 
        let! x = a
        f x
        return x 
    }

let require (f: 'a -> 'b option) msg (x: 'a) = 
    match f x with
    | None -> failwith msg
    | Some y -> y

type RequestContent =
    | Empty
    | String of string
    //| Json of obj
    | File of content: byte[] * filename: string

type Method =
    | GET
    | POST of RequestContent

let call client (uri: string) (meth: Method)  =
    async {
        let action: HttpClient -> Async<HttpResponseMessage>  =
            match meth with
            | GET ->
                fun c -> c.GetAsync(uri) |> Async.AwaitTask
            | POST content ->
                let httpContent = 
                    match content with
                    | Empty -> new ByteArrayContent([||]) :> HttpContent
                    | String s -> new StringContent(s) :> HttpContent
                    //| Json o -> let _, json = serialize o in new ByteArrayContent(json) :> HttpContent
                    | File (content, filename) ->
                        let header = new Headers.ContentDispositionHeaderValue("form-data")
                        header.Name <- "file"
                        header.FileName <- filename

                        let fileContent = new ByteArrayContent(content)
                        fileContent.Headers.ContentDisposition <- header
                        fileContent.Headers.ContentType <- new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream")

                        let httpContent = new MultipartFormDataContent("----WebKitFormBoundary3riXFGfur3u34RN9")
                        httpContent.Add(fileContent)

                        httpContent :> HttpContent

                fun c -> c.PostAsync(uri, httpContent) |> Async.AwaitTask

        return! action client
    }

let getStringBody (response: HttpResponseMessage) = 
    async {
        let! bytes = response.Content.ReadAsByteArrayAsync() |> Async.AwaitTask
        return System.Text.Encoding.ASCII.GetString(bytes)
    }

//let parseJsonBody<'a> (response: HttpResponseMessage) =
//    async {
//        let! bytes = response.Content.ReadAsByteArrayAsync() |> Async.AwaitTask
//        let parsed =
//            try
//                deserializet<'a> bytes |> Choice1Of2
//            with
//            | _ -> Choice2Of2 (System.Text.Encoding.ASCII.GetString(bytes))
//        return parsed
//    }

let getCookie (response: HttpResponseMessage) =
    if response.Headers.Contains("Set-Cookie") then
        let cookieValue = response.Headers.GetValues("Set-Cookie") |> Seq.head
        let cookieParts = cookieValue.Split(';').[0].Split('=')
        Some (cookieParts.[0], cookieParts.[1])
    else
        None

let getRawBody (response: HttpResponseMessage) = 
    async {
        let! bytes = response.Content.ReadAsByteArrayAsync() |> Async.AwaitTask
        return bytes
    }

