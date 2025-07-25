namespace Perla.Server

open Perla.Shared
open System

module Albums =

    let sampleStore = ResizeArray<Album>()

    let findPopular () : Async<Album list> =
        async { return sampleStore |> Seq.filter (fun a -> a.rate > 3u) |> Seq.toList }

    let findAll () : Async<Album list> =
        async { return sampleStore |> Seq.toList }

    let findById (id: Guid) : Async<Album option> =
        async { return sampleStore |> Seq.tryFind (fun a -> a.id = id) }

    let create (args: NewAlbumArgs) : Async<Album option> =
        async {
            let newAlbum =
                { id = Guid.NewGuid()
                  title = args.title
                  artist = args.artist
                  rate = 0u
                  releaseDate = args.releaseDate }

            sampleStore.Add(newAlbum)
            return Some newAlbum
        }

    let update (album: Album) : Async<Album option> =
        async {
            match sampleStore |> Seq.tryFindIndex (fun a -> a.id = album.id) with
            | Some index ->
                sampleStore.[index] <- album
                return Some album
            | None -> return None
        }

    let CreateService () : MusicStore =
        { findPopular = findPopular
          findAll = findAll
          findById = findById
          create = create
          update = update }
