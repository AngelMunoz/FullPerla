module Albums

open Fable.Remoting.Client
open Perla.Shared

let GetStore = Remoting.createApi () |> Remoting.buildProxy<MusicStore>
