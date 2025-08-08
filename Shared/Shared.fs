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


// Audio files shared contract
type AudioTrack = {
  name: string
  size: int64
  lastModified: DateTime
}

// Upload arguments for audio files (metadata; content is sent via multipart form)
type UploadAudioArgs = {
  name: string
}

type MusicStore =
  abstract findPopular: unit -> Async<Album list>
  abstract findAll: unit -> Async<Album list>
  abstract findById: Guid -> Async<Album option>
  abstract create: NewAlbumArgs -> Async<Album option>
  abstract update: Album -> Async<Album option>

// Shared contract for music files operations
type AudioStore =
  abstract list: unit -> Async<AudioTrack list>
  abstract findByName: string -> Async<AudioTrack option>
  // Upload expects the filename and raw file object; transport is multipart
  abstract upload: UploadAudioArgs * obj -> Async<AudioTrack option>
  abstract delete: string -> Async<bool>


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

  module AudioTrack =
    let encode: Encoder<AudioTrack> =
      fun (t: AudioTrack) ->
        Encode.object [
          "name", Encode.string t.name
          "size", Encode.int64 t.size
          "lastModified", Encode.datetime t.lastModified
        ]

    let decode: Decoder<AudioTrack> =
      Decode.object (fun get -> {
        name = get.Required.Field "name" Decode.string
        size = get.Required.Field "size" Decode.int64
        lastModified = get.Required.Field "lastModified" Decode.datetimeUtc
      })

  module AudioTracks =
    let encode: Encoder<AudioTrack list> =
      fun tracks -> Encode.list [ for t in tracks -> AudioTrack.encode t ]

    let decode: Decoder<AudioTrack list> = Decode.list AudioTrack.decode
