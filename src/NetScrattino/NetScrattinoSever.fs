namespace NetScrattino
open Program
open System.Threading.Tasks

type NetScrattinoSever() =
    member x.Start( port ) =
       Server( port ) 
    member x.Start( port, a ) =
       arduino <- a
       Server( port ) 
    member x.StartAsync( port, a ) =
        Task.Factory.StartNew( fun () -> 
            x.Start( port, a )
        )



