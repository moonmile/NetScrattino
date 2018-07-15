namespace FirmataNET

open System
open System.IO.Ports

/// Firmata on Arduino class
/// refer http://firmata.org/wiki/Protocol
type Arduino() =
    let DIGITAL_MESSAGE = 0x90uy    // send data for a digital port
    let ANALOG_MESSAGE = 0xE0uy     // send data for an analog pin (or PWM)
    let REPORT_ANALOG = 0xC0uy      // enable analog input by pin #
    let REPORT_DIGITAL = 0xD0uy     // enable digital input by port
    let SET_PIN_MODE = 0xF4uy       // set a pin to INPUT/OUTPUT/PWM/etc
    let REPORT_VERSION = 0xF9uy     // report firmware version
    let SYSTEM_RESET = 0xFFuy       // reset from MIDI
    let START_SYSEX = 0xF0uy        // start a MIDI SysEx message
    let END_SYSEX = 0xF7uy          // end a MIDI SysEx message

    let mutable _socket:SerialPort = null
    let mutable _portName = "COM3"
    let mutable _baudRate = 57600
 
    // let digitalOutputData = Array.zeroCreate(16)
    let digitalInputData = Array.zeroCreate(16)
    let analogInputData = Array.zeroCreate(16)
 
    /// Connect Serial on Arduino.
    member this.Connect( port:string ) =
        _socket <- new SerialPort( port, _baudRate )
        _socket.Open()
 
    member this.Open() =
        this.Reset()
        // read data form Arduino
        _socket.DataReceived.Add( fun (e) -> 
            while _socket.BytesToRead > 0 do
                let head = _socket.ReadByte() |> byte
                match head with
                // analog 14-bit data format
                // 0  analog pin, 0xE0-0xEF, (MIDI Pitch Wheel)
                // 1  analog least significant 7 bits
                // 2  analog most significant 7 bits
                | h when ANALOG_MESSAGE <= h && h <= ANALOG_MESSAGE + 15uy -> 
                    let pin = int(h - ANALOG_MESSAGE)
                    let lsb = _socket.ReadByte()
                    let msb = _socket.ReadByte()
                    let data = (msb <<< 7) ||| lsb
                    analogInputData.[pin] <- data
                // two byte digital data format, second nibble of byte 0 gives the port number (e.g. 0x92 is the third port, port 2)
                // 0  digital data, 0x90-0x9F, (MIDI NoteOn, but different data format)
                // 1  digital pins 0-6 bitmask
                // 2  digital pin 7 bitmask 
                | h when DIGITAL_MESSAGE <= h && h <= DIGITAL_MESSAGE + 15uy -> 
                    let pin = int(h - DIGITAL_MESSAGE)
                    let lsb = _socket.ReadByte()
                    let msb = _socket.ReadByte()
                    let data = (msb <<< 7) ||| lsb
                    digitalInputData.[pin] <- data
                | _ -> 
                    // read off
                    let d = _socket.ReadExisting()
                    ()
        )
 
    member this.Close() = 
        _socket.Close()
        _socket <- null
 
    member this.Reset() =
        for i=0 to 5 do
            let command = [|
                REPORT_ANALOG ||| byte(i)
                1uy
            |]
            _socket.Write( command, 0, command.Length )
        for i=0 to 1 do
            let command = [|
                REPORT_DIGITAL ||| byte(i)
                1uy
            |]
            _socket.Write( command, 0, command.Length )

    member this.digitalRead(pin:int):int =
        (digitalInputData.[pin >>> 3] >>> (pin &&& 0x07)) &&& 0x01
    member this.analogRead(pin:int):int =
        analogInputData.[pin]
 
    // analog 14-bit data format
    // 0  analog pin, 0xE0-0xEF, (MIDI Pitch Wheel)
    // 1  analog least significant 7 bits
    // 2  analog most significant 7 bits
    member this.pinMode(pin,mode) =
        let message = [|
            SET_PIN_MODE
            byte(pin)
            byte(mode)
        |]
        _socket.Write( message, 0, message.Length )
 
    // two byte digital data format, second nibble of byte 0 gives the port number (e.g. 0x92 is the third port, port 2)
    // 0  digital data, 0x90-0x9F, (MIDI NoteOn, but different data format)
    // 1  digital pins 0-6 bitmask
    // 2  digital pin 7 bitmask 
    member this.digitalWrite(pin,value) =
        let portNumber = (pin >>> 3) &&& 0xFF
        digitalInputData.[portNumber] <-
            if value = 0 then
                digitalInputData.[portNumber] &&& ~~~(1 <<< (pin &&& 0x07))
            else
                digitalInputData.[portNumber] ||| (1 <<< (pin &&& 0x07)) 
        let message = [|
            DIGITAL_MESSAGE ||| byte(portNumber) 
            byte(digitalInputData.[portNumber] &&& 0x7F)
            byte(digitalInputData.[portNumber] >>> 7)
        |]
        _socket.Write(message, 0, message.Length);
             
    // analog 14-bit data format
    // 0  analog pin, 0xE0-0xEF, (MIDI Pitch Wheel)
    // 1  analog least significant 7 bits
    // 2  analog most significant 7 bits
    member this.analogWrite(pin,value) = 
        let message = [|
            ANALOG_MESSAGE ||| (byte(pin) &&& 0x0Fuy)
            byte(value &&& 0x7F)
            byte(value >>> 7)
        |]
        _socket.Write(message, 0, message.Length);
 
    // toggle analogIn reporting by pin
    // 0  toggle analogIn reporting (0xC0-0xCF) (MIDI Program Change)
    // 1  disable(0)/enable(non-zero) 
    member this.analogReport(pin, enable) =
        let message = [|
            REPORT_ANALOG ||| (byte(pin) &&& 0xFuy)
            byte(if enable then 1 else 0)
        |]
        _socket.Write(message, 0, message.Length);

    // toggle digital port reporting by port (second nibble of byte 0), e.g. 0xD1 is port 1 is pins 8 to 15,  
    // 0  toggle digital port reporting (0xD0-0xDF) (MIDI Aftertouch)
    // 1  disable(0)/enable(non-zero) 
    member this.digitalReport(pin, enable) =
        let message = [|
            REPORT_DIGITAL ||| (byte(pin) >>> 3) &&& 0xFFuy
            (1uy <<< (pin &&& 0x07))
        |]
        _socket.Write(message, 0, message.Length);

       
