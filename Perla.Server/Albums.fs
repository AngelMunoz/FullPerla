namespace Perla.Server

open Perla.Shared
open System

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging


module Albums =
  open Microsoft.AspNetCore.Builder
  open Microsoft.AspNetCore.Http

  let sampleStore =
    ResizeArray<Album>(
      [
        {
          id = Guid.NewGuid()
          title = "Album 1"
          artist = "Artist A"
          rate = 4
          releaseDate = DateTime(2020, 1, 1)
        }
        {
          id = Guid.NewGuid()
          title = "Album 2"
          artist = "Artist B"
          rate = 5
          releaseDate = DateTime(2021, 2, 2)
        }
        {
          id = Guid.NewGuid()
          title = "Album 3"
          artist = "Artist C"
          rate = 3
          releaseDate = DateTime(2022, 3, 3)
        }
        {
          id = Guid.NewGuid()
          title = "Album 4"
          artist = "Artist D"
          rate = 2
          releaseDate = DateTime(2023, 4, 4)
        }
        {
          id = Guid.NewGuid()
          title = "Album 5"
          artist = "Artist E"
          rate = 1
          releaseDate = DateTime(2024, 5, 5)
        }
      ]
    )

  let findPopular() : Async<Album list> = async {
    return sampleStore |> Seq.filter(fun a -> a.rate > 3) |> Seq.toList
  }

  let findAll() : Async<Album list> = async { return sampleStore |> Seq.toList }

  let findById(id: Guid) : Async<Album option> = async {
    return sampleStore |> Seq.tryFind(fun a -> a.id = id)
  }

  let create(args: NewAlbumArgs) : Async<Album option> = async {
    let newAlbum = {
      id = Guid.NewGuid()
      title = args.title
      artist = args.artist
      rate = 0
      releaseDate = args.releaseDate
    }

    sampleStore.Add(newAlbum)
    return Some newAlbum
  }

  let update(album: Album) : Async<Album option> = async {
    match sampleStore |> Seq.tryFindIndex(fun a -> a.id = album.id) with
    | Some index ->
      sampleStore.[index] <- album
      return Some album
    | None -> return None
  }

  let CreateService() : MusicStore =
    { new MusicStore with
        member _.findPopular() = findPopular()
        member _.findAll() = findAll()
        member _.findById(id) = findById(id)
        member _.create(args) = create(args)
        member _.update(album) = update(album)
    }

  let RegisterRoutes(app: WebApplication) =
    let group = app.MapGroup "/api/albums"

    group.MapGet(
      "/popular",
      Func<HttpContext, Async<IResult>>(fun ctx -> async {
        let musicStore = ctx.RequestServices.GetService<MusicStore>()

        let! popularAlbums = musicStore.findPopular()
        return Results.Ok(popularAlbums)
      })
    )
    |> ignore

    group.MapGet(
      "/",
      Func<HttpContext, _>(fun ctx -> async {
        let musicStore = ctx.RequestServices.GetService<MusicStore>()

        let! allAlbums = musicStore.findAll()
        return Results.Ok(allAlbums)
      })
    )
    |> ignore

    group.MapPost(
      "/",
      Func<HttpContext, _>(fun ctx -> async {
        let musicStore = ctx.RequestServices.GetService<MusicStore>()

        let! album =
          ctx.Request.ReadFromJsonAsync<NewAlbumArgs>().AsTask()
          |> Async.AwaitTask
          |> Async.Catch

        match album with
        | Choice1Of2 a ->
          let! created = musicStore.create a

          match created with
          | Some c -> return Results.Ok(c)
          | None -> return Results.BadRequest()
        | Choice2Of2 ex -> return Results.BadRequest()
      })
    )
    |> ignore

    group.MapGet(
      "/{id}",
      Func<HttpContext, Guid, _>(fun ctx id -> async {
        let musicStore = ctx.RequestServices.GetService<MusicStore>()

        let! album = musicStore.findById(id)

        match album with
        | Some a -> return Results.Ok(a)
        | None -> return Results.NotFound()
      })
    )
    |> ignore

    group.MapPut(
      "/{id}",
      Func<HttpContext, Guid, _>(fun ctx id -> async {
        let musicStore = ctx.RequestServices.GetService<MusicStore>()
        let! album = musicStore.findById(id)

        match album with
        | Some a ->
          let! updated = musicStore.update a

          match updated with
          | Some u -> return Results.Ok(u)
          | None -> return Results.NotFound()
        | None -> return Results.NotFound()
      })
    )
    |> ignore
