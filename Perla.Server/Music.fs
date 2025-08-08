namespace Perla.Server

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Net.Http.Headers
open Perla.Shared
open Microsoft.Extensions.Primitives

module Music =
  // Physical base directory for audio files. Relative to the server project root.
  let private baseDir = Path.Combine(Directory.GetCurrentDirectory(), "MusicFiles")

  let private ensureBaseDir() =
    if not (Directory.Exists(baseDir)) then
      Directory.CreateDirectory(baseDir) |> ignore

  let private isMp3 (file: string) =
    let ext = Path.GetExtension(file)
    String.Equals(ext, ".mp3", StringComparison.OrdinalIgnoreCase)

  let private listTracks() : AudioTrack list =
    ensureBaseDir()
    Directory.EnumerateFiles(baseDir, "*.mp3")
    |> Seq.map (fun p ->
      let fi = FileInfo(p)
      { name = fi.Name
        size = fi.Length
        lastModified = fi.LastWriteTimeUtc }
    )
    |> Seq.toList

  let private tryGetFilePath (name: string) =
    ensureBaseDir()
    // prevent path traversal
    let sanitized = Path.GetFileName(name)
    let full = Path.Combine(baseDir, sanitized)
    if File.Exists(full) && isMp3 full then Some full else None

  let RegisterRoutes(app: WebApplication) =
    let group = app.MapGroup "/api/music"

    // List available mp3 files
    group.MapGet("/", Func<HttpContext, _>(fun _ -> async {
      let tracks = listTracks()
      return Results.Ok(tracks)
    }))
    |> ignore

    // Stream mp3 file by name
    group.MapGet("/{name}", Func<HttpContext, string, _>(fun ctx name ->
      match tryGetFilePath name with
      | None -> Results.NotFound()
      | Some path ->
        let stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
        let lastWrite = File.GetLastWriteTimeUtc(path)
        let lastModified = DateTimeOffset(lastWrite)
        let etag = sprintf "\"%s-%d\"" (lastWrite.ToFileTimeUtc().ToString()) stream.Length

        // Handle conditional requests
        let headers = ctx.Request.GetTypedHeaders()
        let notModified =
          match headers.IfNoneMatch with
          | null -> false
          | tags when tags.Count > 0 -> tags |> Seq.exists (fun t -> t.Tag = etag)
          | _ -> false

        if notModified then
          stream.Dispose()
          Results.StatusCode(StatusCodes.Status304NotModified)
        else
          let result = Results.Stream(stream, contentType = "audio/mpeg", lastModified = Nullable(lastModified), enableRangeProcessing = true)
          ctx.Response.Headers[HeaderNames.ETag] <- etag
          result
    ))
    |> ignore

    // HEAD for metadata without body
    group.MapMethods("/{name}", [| "HEAD" |], Func<HttpContext, string, _>(fun ctx name ->
      match tryGetFilePath name with
      | None -> Results.NotFound()
      | Some path ->
        let fi = FileInfo(path)
        ctx.Response.ContentType <- "audio/mpeg"
        ctx.Response.ContentLength <- Nullable(fi.Length)
        ctx.Response.Headers[HeaderNames.LastModified] <- File.GetLastWriteTimeUtc(path).ToString("R")
        Results.Ok()
    ))
    |> ignore

    // Upload a new mp3 file via multipart/form-data (field: file, optional name)
    group.MapPost("/", Func<HttpContext, _>(fun ctx -> task {
      ensureBaseDir()
      if not ctx.Request.HasFormContentType then
        return Results.BadRequest("Expected multipart/form-data")
      else
        let! form = ctx.Request.ReadFormAsync()
        let file = form.Files.GetFile("file")
        if isNull file then
          return Results.BadRequest("Missing file field 'file'")
        else
          let nameValues: StringValues = form.["name"]
          let fileName =
            if not (StringValues.IsNullOrEmpty nameValues) then string nameValues.[0] else file.FileName
          let safeName = Path.GetFileName(fileName)
          if not (safeName.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase)) then
            return Results.BadRequest("Only .mp3 files are allowed")
          else
            let destPath = Path.Combine(baseDir, safeName)
            use stream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None)
            do! file.CopyToAsync(stream)
            let fi = FileInfo(destPath)
            let track : AudioTrack = { name = fi.Name; size = fi.Length; lastModified = fi.LastWriteTimeUtc }
            return Results.Ok(track)
    }))
    |> ignore

    // Delete a file by name
    group.MapDelete("/{name}", Func<HttpContext, string, _>(fun _ name ->
      match tryGetFilePath name with
      | None -> Results.NotFound()
      | Some path ->
        File.Delete(path)
        Results.NoContent()
    ))
    |> ignore

