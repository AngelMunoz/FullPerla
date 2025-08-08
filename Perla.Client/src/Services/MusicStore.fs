module App.Services.MusicStore

open Fable.Core
open Perla.Shared
open Thoth.Fetch
open Thoth.Json

[<Literal>]
let AlbumsApiUrl = "/api/albums"

let Extras =
  Extra.empty
  |> Extra.withCustom Codecs.Album.encode Codecs.Album.decode

let private findPopular() = async {
  let! albums =
    promise {
      let! response = Fetch.get($"{AlbumsApiUrl}/popular", extra = Extras, decoder = Codecs.Albums.decode)
      return response
    } |> Async.AwaitPromise
  return albums
}

let private findAll() = async {
  let! albums =
    promise {
      let! response = Fetch.get(AlbumsApiUrl, extra = Extras, decoder = Codecs.Albums.decode)
      return response
    } |> Async.AwaitPromise
  return albums
}

let private findById id = async {
  let! albumOpt =
    promise {
      let! response = Fetch.get($"{AlbumsApiUrl}/{id}", decoder = Codecs.Album.Option.decode)
      return response
    } |> Async.AwaitPromise
  return albumOpt
}

let private create args = async {
  let! albumOpt =
    promise {
      let! response = Fetch.post(AlbumsApiUrl, Codecs.AlbumArgs.encode args, decoder = Codecs.Album.Option.decode)
      return response
    } |> Async.AwaitPromise
  return albumOpt
}

let private update album = async {
  let! updatedOpt =
    promise {
      let! response = Fetch.put($"{AlbumsApiUrl}/{album.id}", Codecs.Album.encode album, decoder = Codecs.Album.Option.decode)
      return response
    } |> Async.AwaitPromise
  return updatedOpt
}

let CreateService() : MusicStore =
  { new MusicStore with
      member _.findPopular() = findPopular()
      member _.findAll() = findAll()
      member _.findById(id) = findById id
      member _.create(args) = create args
      member _.update(album) = update album
  }
