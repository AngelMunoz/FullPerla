module Albums

open Perla.Shared

open Fable.Core
open Thoth.Fetch
open Thoth.Json


[<Literal>]
let AlbumsApiUrl = "/api/albums"

let Extras = Extra.empty |> Extra.withCustom Codecs.Album.encode Codecs.Album.decode


let findPopular () =
    async {
        let! albulist =
            promise {
                let! response = Fetch.get ($"{AlbumsApiUrl}/popular", extra = Extras, decoder = Codecs.Albums.decode)

                return response
            }
            |> Async.AwaitPromise

        return albulist
    }

let findAll () =
    async {
        let! albulist =
            promise {
                let! response = Fetch.get (AlbumsApiUrl, decoder = Codecs.Albums.decode)

                return response
            }
            |> Async.AwaitPromise

        return albulist
    }

let findById id =
    async {
        let! albumOption =
            promise {
                let! response = Fetch.get ($"{AlbumsApiUrl}/{id}", decoder = Codecs.Album.Option.decode)

                return response
            }
            |> Async.AwaitPromise

        return albumOption
    }

let create args =
    async {
        let! albumOption =
            promise {
                let! response =
                    Fetch.post (AlbumsApiUrl, Codecs.AlbumArgs.encode args, decoder = Codecs.Album.Option.decode)

                return response
            }
            |> Async.AwaitPromise

        return albumOption
    }

let update album =
    async {
        let! updatedAlbumOption =
            promise {
                let! response =
                    Fetch.put (
                        $"{AlbumsApiUrl}/{album.id}",
                        Codecs.Album.encode album,
                        decoder = Codecs.Album.Option.decode
                    )

                return response
            }
            |> Async.AwaitPromise

        return updatedAlbumOption
    }


let CreateService () : MusicStore =
    { new MusicStore with
        member _.findPopular() = findPopular ()
        member _.findAll() = findAll ()
        member _.findById(id) = findById id
        member _.create(args) = create args
        member _.update(album) = update album }
