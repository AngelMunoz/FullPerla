module App.Services.AudioStore

open Fable.Core
open Fable.Core.JsInterop
open Thoth.Fetch
open Thoth.Json
open Perla.Shared
open Fetch.Types

[<Literal>]
let MusicApiUrl = "/api/music"

let Extras =
  Extra.empty
  |> Extra.withCustom Codecs.AudioTrack.encode Codecs.AudioTrack.decode
  |> Extra.withCustom Codecs.AudioTracks.encode Codecs.AudioTracks.decode

// Native helpers for multipart upload without extra packages
[<Emit("new FormData()")>]
let private newFormData () : obj = jsNative

[<Emit("$0.append($1,$2)")>]
let private formAppendFile (form: obj) (name: string) (file: obj) : unit = jsNative

[<Emit("$0.append($1,$2)")>]
let private formAppendText (form: obj) (name: string) (value: string) : unit = jsNative

[<Emit("fetch($0, $1)")>]
let private fetch2 (url: string, init: obj) : JS.Promise<obj> = jsNative

let private list () = async {
  let! tracks =
    promise {
      let! response = Fetch.get(MusicApiUrl, extra = Extras, decoder = Codecs.AudioTracks.decode)
      return response
    } |> Async.AwaitPromise
  return tracks
}

let private findByName (name: string) = async {
  let! tracks = list()
  return tracks |> List.tryFind (fun t -> t.name = name)
}

let private upload (args: UploadAudioArgs, file: obj) = async {
  let form = newFormData()
  formAppendText form "name" args.name
  formAppendFile form "file" file

  let init =
    createObj [
      "method" ==> "POST"
      "body" ==> form
    ]

  let! resObj = fetch2(MusicApiUrl, init) |> Async.AwaitPromise
  let ok : bool = resObj?ok
  if not ok then
    return None
  else
    let! (txt : string) = resObj?text() |> Async.AwaitPromise
    match Decode.fromString Codecs.AudioTrack.decode txt with
    | Ok track -> return Some track
    | Error _ -> return None
}

let private delete (name: string) = async {
  let url = sprintf "%s/%s" MusicApiUrl name
  let init = createObj [ "method" ==> "DELETE" ]
  let! resObj = fetch2(url, init) |> Async.AwaitPromise
  let ok : bool = resObj?ok
  return ok
}

let CreateService () : AudioStore =
  { new AudioStore with
      member _.list() = list()
      member _.findByName(name) = findByName name
      member _.upload(args, file) = upload (args, file)
      member _.delete(name) = delete name
  }
