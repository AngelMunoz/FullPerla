module App.Pages.MusicPlayer

open Feliz
open Feliz.UseElmish
open Elmish
open Perla.Shared
open Fable.Core
open Fable.Core.JsInterop

[<Emit("encodeURIComponent($0)")>]
let private encodeUri (s: string) : string = jsNative

[<Emit("$0 && $0.target && $0.target.files && $0.target.files[0]")>]
let private getFileFromEvent (ev: obj) : obj = jsNative

[<Emit("$0 && $0.dataTransfer && $0.dataTransfer.files && $0.dataTransfer.files")>]
let private getFilesFromDrop (ev: obj) : obj = jsNative

let private formatBytes (bytes: int64) =
  let b = float bytes
  if b < 1024.0 then sprintf "%.0f B" b
  elif b < 1024.0 ** 2.0 then sprintf "%.1f KB" (b / 1024.0)
  elif b < 1024.0 ** 3.0 then sprintf "%.1f MB" (b / (1024.0 ** 2.0))
  else sprintf "%.1f GB" (b / (1024.0 ** 3.0))

let private formatTime (sec: float) =
  if System.Double.IsNaN sec || System.Double.IsInfinity sec then "0:00"
  else
    let total = int (System.Math.Floor sec)
    let m = total / 60
    let s = total % 60
    sprintf "%d:%02d" m s

type State = {
  Tracks: AudioTrack list
  Selected: AudioTrack option
  IsLoading: bool
  IsUploading: bool
  Error: string option
  CurrentTime: float
  Duration: float
  IsPlaying: bool
  Query: string
  Volume: float
  Looping: bool
}

type Msg =
  | Load
  | SetTracks of AudioTrack list
  | Select of AudioTrack
  | SetError of string option
  | UploadStart
  | UploadDone of AudioTrack option
  | DeleteSelected
  | Deleted of bool
  | TimeUpdate of float
  | MetadataLoaded of float
  | Play
  | Pause
  | SetQuery of string
  | Prev
  | Next
  | Seek of float
  | SetVolume of float
  | ToggleLoop
  | Ended

let init() : State * Cmd<Msg> =
  { Tracks = []
    Selected = None
    IsLoading = true
    IsUploading = false
    Error = None
    CurrentTime = 0.0
    Duration = 0.0
    IsPlaying = false
    Query = ""
    Volume = 1.0
    Looping = false }, Cmd.ofMsg Load

let update (store: AudioStore) (msg: Msg) (state: State) : State * Cmd<Msg> =
  match msg with
  | Load ->
    { state with IsLoading = true },
    Cmd.OfAsync.either (fun () -> store.list()) () (fun tracks -> SetTracks tracks) (fun ex -> SetError (Some ex.Message))
  | SetTracks tracks ->
    let selected = match tracks, state.Selected with | _ , Some sel -> tracks |> List.tryFind (fun t -> t.name = sel.name) | _ -> tracks |> List.tryHead
    { state with Tracks = tracks; Selected = selected; IsLoading = false }, Cmd.none
  | Select t -> { state with Selected = Some t; CurrentTime = 0.0; Duration = 0.0; IsPlaying = true }, Cmd.none
  | SetError e -> { state with Error = e; IsLoading = false; IsUploading = false }, Cmd.none
  | UploadStart -> { state with IsUploading = true; Error = None }, Cmd.none
  | UploadDone opt ->
    match opt with
    | None -> { state with IsUploading = false; Error = Some "Upload failed" }, Cmd.none
    | Some _ -> { state with IsUploading = false }, Cmd.ofMsg Load
  | DeleteSelected ->
    match state.Selected with
    | None -> state, Cmd.none
    | Some sel ->
      state,
      Cmd.OfAsync.either (fun name -> store.delete name) sel.name (fun ok -> Deleted ok) (fun ex -> SetError (Some ex.Message))
  | Deleted ok ->
    if ok then state, Cmd.ofMsg Load else { state with Error = Some "Delete failed" }, Cmd.none
  | TimeUpdate t -> { state with CurrentTime = t }, Cmd.none
  | MetadataLoaded d -> { state with Duration = d }, Cmd.none
  | Play -> { state with IsPlaying = true }, Cmd.none
  | Pause -> { state with IsPlaying = false }, Cmd.none
  | SetQuery q -> { state with Query = q }, Cmd.none
  | Prev ->
    match state.Selected, state.Tracks with
    | None, _ -> state, Cmd.none
    | Some sel, tracks ->
      match tracks |> List.tryFindIndex (fun t -> t.name = sel.name) with
      | None -> state, Cmd.none
      | Some idx ->
        let prevIdx = if idx > 0 then idx - 1 else List.length tracks - 1
        let nextSel = tracks |> List.item prevIdx
        { state with Selected = Some nextSel; CurrentTime = 0.0; IsPlaying = true }, Cmd.none
  | Next ->
    match state.Selected, state.Tracks with
    | None, _ -> state, Cmd.none
    | Some sel, tracks ->
      match tracks |> List.tryFindIndex (fun t -> t.name = sel.name) with
      | None -> state, Cmd.none
      | Some idx ->
        let nextIdx = if idx < List.length tracks - 1 then idx + 1 else 0
        let nextSel = tracks |> List.item nextIdx
        { state with Selected = Some nextSel; CurrentTime = 0.0; IsPlaying = true }, Cmd.none
  | Seek v -> { state with CurrentTime = v }, Cmd.none
  | SetVolume v -> { state with Volume = v }, Cmd.none
  | ToggleLoop -> { state with Looping = not state.Looping }, Cmd.none
  | Ended -> if state.Looping then state, Cmd.none else ({ state with IsPlaying = false }, Cmd.ofMsg Next)

[<ReactComponent>]
let View(store: AudioStore) =
  let state, dispatch = React.useElmish(init, update store)
  let audioRef = React.useRef(None: Browser.Types.HTMLAudioElement option)

  React.useEffect(
    (fun () ->
      match audioRef.current with
      | Some el when state.IsPlaying -> el.play() |> ignore
      | Some el when not state.IsPlaying -> el.pause()
      | _ -> ()
      None),
    [| box state.IsPlaying |]
  )

  React.useEffect((fun () -> dispatch Load; None), [||])

  let onFileChange (ev: Browser.Types.Event) =
    promise {
      let file = getFileFromEvent ev
      if isNull file then return () else
      dispatch UploadStart
      let name: string = file?name
      let args = { name = name }
      let! uploaded = store.upload(args, file) |> Async.StartAsPromise
      dispatch (UploadDone uploaded)
    } |> ignore

  let onPlayClick _ =
    match audioRef.current with
    | Some el -> el.play() |> ignore; dispatch Play
    | None -> ()

  let onPauseClick _ =
    match audioRef.current with
    | Some el -> el.pause(); dispatch Pause
    | None -> ()

  let onTimeUpdate (_: Browser.Types.Event) =
    match audioRef.current with
    | Some el -> dispatch (TimeUpdate el.currentTime)
    | None -> ()

  let onLoadedMetadata (_: Browser.Types.Event) =
    match audioRef.current with
    | Some el -> dispatch (MetadataLoaded el.duration)
    | None -> ()

  let onSeekChange (v: float) =
    match audioRef.current with
    | Some el -> el.currentTime <- v; dispatch (Seek v)
    | None -> ()

  let onVolumeChange (v: float) =
    match audioRef.current with
    | Some el -> el.volume <- v; dispatch (SetVolume v)
    | None -> ()

  let onEnded (_: Browser.Types.Event) = dispatch Ended

  let onDrop (ev: Browser.Types.Event) =
    ev.preventDefault()
    promise {
      let files = getFilesFromDrop ev
      if isNull files then return () else
      let file: obj = files?item(0)
      if isNull file then return () else
      dispatch UploadStart
      let name: string = file?name
      let args = { name = name }
      let! uploaded = store.upload(args, file) |> Async.StartAsPromise
      dispatch (UploadDone uploaded)
    } |> ignore

  let onDragOver (ev: Browser.Types.Event) = ev.preventDefault()

  let filtered =
    if System.String.IsNullOrWhiteSpace state.Query then state.Tracks
    else state.Tracks |> List.filter (fun t -> t.name.ToLower().Contains(state.Query.ToLower()))

  let player =
    match state.Selected with
    | None -> Html.div [ Html.p "No track selected" ]
    | Some t ->
      let src = $"/api/music/{encodeUri t.name}"
      Html.div [
        prop.className "music-player-card"
        prop.children [
          Html.div [ prop.className "track-title"; prop.text t.name ]
          Html.audio [
            prop.src src
            prop.controls false
            prop.ref audioRef
            prop.onTimeUpdate onTimeUpdate
            prop.onLoadedMetadata onLoadedMetadata
            prop.onEnded onEnded
            prop.loop state.Looping
          ]
          Html.div [
            prop.className "progress-row"
            prop.children [
              Html.span [ prop.className "time"; prop.text (formatTime state.CurrentTime) ]
              Html.input [
                prop.className "progress"
                prop.type' "range"
                prop.min 0
                prop.max (if state.Duration > 0 then state.Duration else 0)
                prop.step 0.1
                prop.value state.CurrentTime
                prop.onChange (fun (v: float) -> onSeekChange v)
              ]
              Html.span [ prop.className "time"; prop.text (formatTime state.Duration) ]
            ]
          ]
          Html.div [
            prop.className "controls-row"
            prop.children [
              Html.button [ prop.className "btn icon"; prop.text "⏮"; prop.onClick (fun _ -> dispatch Prev) ]
              Html.button [ prop.className "btn primary"; prop.text (if state.IsPlaying then "Pause" else "Play"); prop.onClick (if state.IsPlaying then onPauseClick else onPlayClick) ]
              Html.button [ prop.className "btn icon"; prop.text "⏭"; prop.onClick (fun _ -> dispatch Next) ]
              Html.button [ prop.className (if state.Looping then "btn toggle active" else "btn toggle"); prop.text "Loop"; prop.onClick (fun _ -> dispatch ToggleLoop) ]
              Html.div [
                prop.className "volume"
                prop.children [
                  Html.span [ prop.text "🔊" ]
                  Html.input [
                    prop.type' "range"
                    prop.min 0
                    prop.max 1
                    prop.step 0.01
                    prop.value state.Volume
                    prop.onChange (fun (v: float) -> onVolumeChange v)
                  ]
                ]
              ]
              Html.button [ prop.className "btn danger"; prop.text "Delete"; prop.onClick (fun _ -> if Browser.Dom.window.confirm("Delete this track?") then dispatch DeleteSelected); prop.disabled state.IsUploading ]
            ]
          ]
        ]
      ]

  Html.section [
    prop.className "music-layout"
    prop.children [
      Html.aside [
        prop.className "music-sidebar"
        prop.children [
          Html.div [
            prop.className "sidebar-header"
            prop.children [
              Html.h2 [ prop.text "Library" ]
              Html.p [ prop.className "muted"; prop.text (sprintf "%d tracks" state.Tracks.Length) ]
              Html.input [
                prop.className "search"
                prop.placeholder "Search tracks..."
                prop.value state.Query
                prop.onChange (fun (v: string) -> dispatch (SetQuery v))
              ]
            ]
          ]
          Html.ul [
            prop.className "track-list"
            prop.children [
              for t in filtered ->
                Html.li [
                  prop.className ("track-item " + (if Some t = state.Selected then "active" else ""))
                  prop.onClick (fun _ -> dispatch (Select t))
                  prop.children [
                    Html.span [ prop.className "name"; prop.title t.name; prop.text t.name ]
                    Html.span [ prop.className "meta"; prop.text (formatBytes t.size) ]
                  ]
                ]
            ]
          ]
        ]
      ]
      Html.main [
        prop.className "music-main"
        prop.children [
          player
          Html.div [
            prop.className "uploader"
            prop.onDragOver onDragOver
            prop.onDrop onDrop
            prop.children [
              Html.div [ prop.className "dropzone"; prop.text "Drag & drop MP3 here or choose a file" ]
              Html.input [ prop.type' "file"; prop.accept ".mp3"; prop.onChange onFileChange ]
              if state.IsUploading then Html.p [ prop.className "muted"; prop.text "Uploading..." ]
            ]
          ]
        ]
      ]
    ]
  ]
