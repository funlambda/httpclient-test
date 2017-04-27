open System.Net.Http
open Http

let test1 call = async {
    let! page = 
        call "http://www.cnn.com" GET
        |~> getStringBody

    return page
}

[<EntryPoint>]
let main argv = 
    let client = new HttpClient()
    let call = call client
    
    let res = test1 call |> Async.RunSynchronously

    printfn "%A" res
    0 // return an integer exit code
