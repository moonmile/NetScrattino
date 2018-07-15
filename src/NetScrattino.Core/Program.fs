module Program

open System

let mutable arduino = new FirmataNET.Arduino()

// Scratchから受信するためのHTTPサーバー
let Server( port ) =
    let listener = new System.Net.HttpListener()
    listener.Prefixes.Add("http://127.0.0.1:"+(port |> string)+"/" )
    listener.Start()
    while true do
        let context = listener.GetContext()
        let res = context.Response
        let mutable data = ""
        let path = context.Request.Url.PathAndQuery
        match path with 
            | "/poll" -> 
                for i=0 to 5 do 
                    data <- data + String.Format("a{0} {1}\n", i, arduino.analogRead(i))
                for i=2 to 13 do 
                    data <- data + String.Format("d{0} {1}\n", i, arduino.digitalRead(i))
                // デバッグ出力
                let mutable debug = ""
                for i=0 to 5 do 
                    debug <- debug + String.Format("a{0} {1} ", i, arduino.analogRead(i))
                debug <- debug + "\n"
                for i=2 to 13 do 
                    debug <- debug + String.Format("d{0} {1} ", i, arduino.digitalRead(i))
                debug <- debug + "\n"
                // printfn "%s" path
                // printfn "%s" debug
            | "/reset_all" ->
                printfn "/reset_all"
                arduino.Reset()
                data <- "ok"
            | _ ->
                let pa = path.Split([|'/'|])
                match pa.[1] with   
                | "digitalWrite" ->
                    let pin = pa.[2].Substring(1) |> int
                    let value = 
                        match pa.[3].ToUpper() with
                        | "ON" -> 1
                        | "OFF" -> 0
                        | _ -> pa.[3] |> int
                    arduino.digitalWrite( pin, value )
                | "analogWrite" ->
                    let pin = pa.[2].Substring(1) |> int
                    let value = pa.[3] |> int
                    arduino.pinMode( pin, 0x03 )    // PWM
                    arduino.analogWrite( pin, value )
                | "servoWrite" ->
                    let pin = pa.[2].Substring(1) |> int
                    let value = pa.[3] |> int
                    arduino.pinMode( pin, 0x04 )    // SERVO
                    arduino.analogWrite( pin, value )
                | "setMode" ->
                    let pin = pa.[2].Substring(1) |> int
                    let value = if pa.[3] = "PULLUP" then 0x0B else 0x00
                    arduino.pinMode( pin, value )
                | "setPinMode" ->
                    let pin = pa.[2].Substring(1) |> int
                    let value = 
                        match pa.[3] with
                        | "INPUT" -> 0
                        | "OUTPUT" -> 1
                        | "PWM" -> 3
                        | "SERVO" -> 4
                        | "INPUT_PULLUP" -> 11
                        | _ -> 1
                    arduino.pinMode( pin, value )
                | _ ->
                    data <- ""
                printfn "%s" path
        res.StatusCode <- 200
        let sw = new System.IO.StreamWriter( res.OutputStream )
        sw.Write( data )
        sw.Close()
    ()


