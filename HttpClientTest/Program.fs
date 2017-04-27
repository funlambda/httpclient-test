open System.Net.Http

[<EntryPoint>]
let main argv = 
    let client = new HttpClient()
    let response = client.GetAsync("http://www.google.com/") |> Async.AwaitTask |> Async.RunSynchronously
    let str = response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
    printfn "%A" str
    0 // return an integer exit code
