module Albums

open Fable.Remoting.Client
open Perla.Shared

let store =
    { findPopular =
        fun () ->
            async {
                // Simulate fetching popular albums
                return
                    [ { id = System.Guid.NewGuid()
                        title = "Album 1"
                        artist = "Artist A"
                        rate = 5
                        releaseDate = System.DateTime.Now }
                      { id = System.Guid.NewGuid()
                        title = "Album 2"
                        artist = "Artist B"
                        rate = 4
                        releaseDate = System.DateTime.Now } ]
            }
      findAll =
        fun () ->
            async {
                // Simulate fetching all albums
                return []
            }
      findById =
        fun _ ->
            async {
                // Simulate finding an album by ID
                return None
            }
      create =
        fun _ ->
            async {
                // Simulate creating a new album
                return None
            }
      update =
        fun _ ->
            async {
                // Simulate updating an album
                return None
            } }

let GetStore = Remoting.createApi () |> Remoting.buildProxy<MusicStore>
