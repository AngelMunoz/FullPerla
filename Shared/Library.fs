namespace Perla.Shared

open System

type Album =
    { id: Guid
      title: string
      artist: string
      rate: int
      releaseDate: DateTime }

type NewAlbumArgs =
    { title: string
      artist: string
      releaseDate: DateTime }

type MusicStore =
    { findPopular: unit -> Async<Album list>
      findAll: unit -> Async<Album list>
      findById: Guid -> Async<Album option>
      create: NewAlbumArgs -> Async<Album option>
      update: Album -> Async<Album option> }
