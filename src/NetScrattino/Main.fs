open Program
open System

[<EntryPoint>]
let main argv = 
    let port = 
        if ( argv.Length = 0 ) then
            "COM3"
        else
            argv.[0]

    printfn "Start .NET Scrattino Server"
    arduino.Connect( port )
    arduino.Open()
    Server(5410)
    Console.ReadKey() |> ignore   
    0 


