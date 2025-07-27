namespace Perla.Shared

open System

type Album = {
  id: Guid
  title: string
  artist: string
  rate: int
  releaseDate: DateTime
}

type NewAlbumArgs = {
  title: string
  artist: string
  releaseDate: DateTime
}

type MusicStore =
  abstract findPopular: unit -> Async<Album list>
  abstract findAll: unit -> Async<Album list>
  abstract findById: Guid -> Async<Album option>
  abstract create: NewAlbumArgs -> Async<Album option>
  abstract update: Album -> Async<Album option>


module Codecs =
#if FABLE_COMPILER
  open Thoth.Json
#else
  open Thoth.Json.Net
#endif

  module Album =
    let encode: Encoder<Album> =
      fun (album: Album) ->
        Encode.object [
          "id", Encode.guid album.id
          "title", Encode.string album.title
          "artist", Encode.string album.artist
          "rate", Encode.int album.rate
          "releaseDate", Encode.datetime album.releaseDate
        ]

    let decode: Decoder<Album> =
      Decode.object(fun get -> {
        id = get.Required.Field "id" Decode.guid
        title = get.Required.Field "title" Decode.string
        artist = get.Required.Field "artist" Decode.string
        rate = get.Required.Field "rate" Decode.int
        releaseDate = get.Required.Field "releaseDate" Decode.datetimeUtc
      })


    module Option =
      let decode: Decoder<Album option> = Decode.option decode

  module Albums =
    let encode: Encoder<Album list> =
      fun albums -> Encode.list [ for album in albums -> Album.encode album ]

    let decode: Decoder<Album list> = Decode.list Album.decode

  module AlbumArgs =
    let encode: Encoder<NewAlbumArgs> =
      fun (args: NewAlbumArgs) ->
        Encode.object [
          "title", Encode.string args.title
          "artist", Encode.string args.artist
          "releaseDate", Encode.datetime args.releaseDate
        ]
