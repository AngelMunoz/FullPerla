module Albums

open Perla.Shared

open Fable.Core
open Thoth.Fetch
open Thoth.Json


[<Literal>]
let AlbumsApiUrl = "/api/albums"

let Extras =
  Extra.empty |> Extra.withCustom Codecs.Album.encode Codecs.Album.decode


let findPopular() = async {
  let! albulist =
    promise {
      let! response =
        Fetch.get(
          $"{AlbumsApiUrl}/popular",
          extra = Extras,
          decoder = Codecs.Albums.decode
        )

      return response
    }
    |> Async.AwaitPromise

  return albulist
}

let findAll() = async {
  let! albulist =
    promise {
      let! response =
        Fetch.get(AlbumsApiUrl, extra = Extras, decoder = Codecs.Albums.decode)

      return response
    }
    |> Async.AwaitPromise

  return albulist
}

let findById id = async {
  let! albumOption =
    promise {
      let! response =
        Fetch.get($"{AlbumsApiUrl}/{id}", decoder = Codecs.Album.Option.decode)

      return response
    }
    |> Async.AwaitPromise

  return albumOption
}

let create args = async {
  let! albumOption =
    promise {
      let! response =
        Fetch.post(
          AlbumsApiUrl,
          Codecs.AlbumArgs.encode args,
          decoder = Codecs.Album.Option.decode
        )

      return response
    }
    |> Async.AwaitPromise

  return albumOption
}

let update album = async {
  let! updatedAlbumOption =
    promise {
      let! response =
        Fetch.put(
          $"{AlbumsApiUrl}/{album.id}",
          Codecs.Album.encode album,
          decoder = Codecs.Album.Option.decode
        )

      return response
    }
    |> Async.AwaitPromise

  return updatedAlbumOption
}


let CreateService() : MusicStore =
  { new MusicStore with
      member _.findPopular() = findPopular()
      member _.findAll() = findAll()
      member _.findById(id) = findById id
      member _.create(args) = create args
      member _.update(album) = update album
  }


module View =
  open Feliz
  open Feliz.UseElmish
  open Elmish



  module Form =
    type AlbumFormState = {
      Title: string
      Artist: string
      ReleaseDate: string
    }

    type AlbumFormMsg =
      | SetTitle of string
      | SetArtist of string
      | SetReleaseDate of string
      | Submit

    let albumFormInit
      (albumOpt: Album option)
      : AlbumFormState * Cmd<AlbumFormMsg> =
      let title =
        albumOpt |> Option.map(fun a -> a.title) |> Option.defaultValue ""

      let artist =
        albumOpt |> Option.map(fun a -> a.artist) |> Option.defaultValue ""

      let releaseDate =
        albumOpt
        |> Option.map(fun a -> a.releaseDate.ToString("yyyy-MM-dd"))
        |> Option.defaultValue ""

      {
        Title = title
        Artist = artist
        ReleaseDate = releaseDate
      },
      Cmd.none

    let albumFormUpdate
      (onSubmit: NewAlbumArgs -> unit)
      (msg: AlbumFormMsg)
      (state: AlbumFormState)
      : AlbumFormState * Cmd<AlbumFormMsg> =
      match msg with
      | SetTitle t -> { state with Title = t }, Cmd.none
      | SetArtist a -> { state with Artist = a }, Cmd.none
      | SetReleaseDate d -> { state with ReleaseDate = d }, Cmd.none
      | Submit ->
        let mutable dt = System.DateTime.Now
        let _ = System.DateTime.TryParse(state.ReleaseDate, &dt)

        onSubmit {
          title = state.Title
          artist = state.Artist
          releaseDate = dt
        }

        state, Cmd.none

    [<ReactComponent>]
    let AlbumForm(albumOpt: Album option, onSubmit: NewAlbumArgs -> unit) =
      let state, dispatch =
        React.useElmish(albumFormInit albumOpt, albumFormUpdate onSubmit)

      Html.form [
        prop.onSubmit(fun e ->
          e.preventDefault()
          dispatch Submit)
        prop.children [
          Html.input [
            prop.value state.Title
            prop.onChange(SetTitle >> dispatch)
            prop.placeholder "Title"
          ]
          Html.input [
            prop.value state.Artist
            prop.onChange(SetArtist >> dispatch)
            prop.placeholder "Artist"
          ]
          Html.input [
            prop.value state.ReleaseDate
            prop.onChange(SetReleaseDate >> dispatch)
            prop.placeholder "Release Date (yyyy-MM-dd)"
            prop.type' "date"
          ]
          Html.button [ prop.type' "submit"; prop.text "Save" ]
        ]
      ]

  type View =
    | Full of Album list
    | Popular of Album list
    | Detail of Album
    | Update of Album
    | Create
    | NotFound of string

  type State = {
    View: View
    Albums: Album list
    SelectedAlbum: Album option
    ErrorMessage: string option
    IsLoading: bool
  }

  type Msg =
    | SetView of View
    | SetAlbums of Album list
    | SetSelectedAlbum of Album option
    | SetErrorMessage of string option
    | SetLoading of bool
    | AddCreated of Album
    | AddUpdated of Album
    | LoadPopular
    | RequestAllAlbums
    | RequestViewDetail of System.Guid
    | RequestUpdateAlbum of NewAlbumArgs * Album
    | RequestCreateAlbum of NewAlbumArgs
    | RequestDeleteAlbum of System.Guid

  let init() : State * Cmd<Msg> =
    {
      View = Full []
      Albums = []
      SelectedAlbum = None
      ErrorMessage = None
      IsLoading = false
    },
    Cmd.ofMsg LoadPopular

  let update (store: MusicStore) (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | SetView view -> { state with View = view }, Cmd.none
    | SetAlbums albums ->
      {
        state with
            Albums = albums
            IsLoading = false
      },
      Cmd.none
    | SetSelectedAlbum album ->
      {
        state with
            SelectedAlbum = album
            IsLoading = false
      },
      Cmd.none
    | SetErrorMessage error ->
      {
        state with
            ErrorMessage = error
            IsLoading = false
      },
      Cmd.none
    | SetLoading isLoading -> { state with IsLoading = isLoading }, Cmd.none
    | AddCreated album ->
      {
        state with
            IsLoading = false
            Albums = album :: state.Albums
            View = Detail album
      },
      Cmd.none
    | AddUpdated album ->
      let albums =
        state.Albums |> List.map(fun a -> if a.id = album.id then album else a)

      {
        state with
            IsLoading = false
            Albums = albums
            View = Detail album
      },
      Cmd.none
    | LoadPopular ->
      { state with IsLoading = true },
      Cmd.OfAsync.either
        store.findPopular
        ()
        (fun albums -> SetView(Popular albums))
        (fun ex -> SetErrorMessage(Some ex.Message))
    | RequestViewDetail albumId ->
      state,
      Cmd.OfAsync.either
        (fun id -> store.findById id)
        albumId
        (function
        | Some a -> SetView(Detail a)
        | None -> SetView(NotFound "Album not found"))
        (fun ex -> SetErrorMessage(Some ex.Message))
    | RequestUpdateAlbum(args, album) ->
      state,
      Cmd.OfAsync.either
        (fun (args, album) ->
          store.update {
            album with
                title = args.title
                artist = args.artist
                releaseDate = args.releaseDate
          })
        (args, album)
        (function
        | Some updated -> AddUpdated updated
        | None -> SetErrorMessage(Some "Unable to update album"))
        (fun ex -> SetErrorMessage(Some ex.Message))
    | RequestCreateAlbum args ->
      state,
      Cmd.OfAsync.either
        store.create
        args
        (function
        | Some album -> AddCreated album
        | None -> SetErrorMessage(Some "Unable to create album"))
        (fun ex -> SetErrorMessage(Some ex.Message))
    | RequestDeleteAlbum albumId ->
      let albums = state.Albums |> List.filter(fun a -> a.id <> albumId)
      { state with Albums = albums }, Cmd.none
    | RequestAllAlbums ->
      { state with IsLoading = true },
      Cmd.OfAsync.either
        store.findAll
        ()
        (fun albums -> SetView(Full albums))
        (fun ex -> SetErrorMessage(Some ex.Message))

  let albumListView
    (albums: Album list)
    (musicStore: MusicStore)
    (dispatch: Msg -> unit)
    (state: State)
    =
    Html.ul(
      albums
      |> List.map(fun album ->
        Html.li [
          prop.text(album.title + " by " + album.artist)
          prop.children [
            Html.button [
              prop.text "View"
              prop.onClick(fun _ ->
                async {
                  let! found = musicStore.findById(album.id)

                  match found with
                  | Some a -> dispatch(SetView(Detail a))
                  | None -> dispatch(SetView(NotFound "Album not found"))
                }
                |> Async.Start)
            ]
            Html.button [
              prop.text "Edit"
              prop.onClick(fun _ -> dispatch(SetView(Update album)))
            ]
            Html.button [
              prop.text "Delete"
              prop.onClick(fun _ ->
                async {
                  let albums =
                    state.Albums |> List.filter(fun a -> a.id <> album.id)

                  dispatch(SetAlbums albums)
                }
                |> Async.Start)
            ]
          ]
        ])
    )

  [<ReactComponent>]
  let AlbumListItem(album: Album, dispatch: Msg -> unit) =
    Html.li [
      prop.children [
        Html.span [ prop.text(album.title + " by " + album.artist) ]
        Html.button [
          prop.text "View"
          prop.onClick(fun _ -> dispatch(RequestViewDetail album.id))
        ]
        Html.button [
          prop.text "Edit"
          prop.onClick(fun _ -> dispatch(SetView(Update album)))
        ]
        Html.button [
          prop.text "Delete"
          prop.onClick(fun _ -> dispatch(RequestDeleteAlbum album.id))
        ]
      ]
    ]

  [<ReactComponent>]
  let AlbumListView(children: ReactElement seq) = Html.ul children

  let albumDetailView (album: Album) (dispatch: Msg -> unit) (state: State) =
    Html.div [
      Html.h2 album.title
      Html.p("Artist: " + album.artist)
      Html.p("Release Date: " + album.releaseDate.ToString("yyyy-MM-dd"))
      Html.button [
        prop.text "Edit"
        prop.onClick(fun _ -> dispatch(SetView(Update album)))
      ]
      Html.button [
        prop.text "Back"
        prop.onClick(fun _ -> dispatch(SetView(Full state.Albums)))
      ]
    ]

  let notFoundView(msg: string) =
    Html.div [ Html.h2 "Not Found"; Html.p msg ]

  [<ReactComponent>]
  let AlbumsShowcase(musicStore: MusicStore) =
    let state, dispatch = React.useElmish(init, update musicStore)

    Html.section [
      prop.className "albums-layout"
      prop.children [
        Html.nav [
          prop.className "albums-sidebar"
          prop.children [
            Html.h2 [
              prop.text "Albums"
              prop.className "albums-sidebar-title"
            ]
            Html.button [
              prop.text "Popular"
              prop.className "albums-sidebar-btn"
              prop.onClick(fun _ -> dispatch(LoadPopular))
              prop.disabled(
                match state.View with
                | Popular _ -> true
                | _ -> false
              )
            ]
            Html.button [
              prop.text "All Albums"
              prop.className "albums-sidebar-btn"
              prop.onClick(fun _ -> dispatch(RequestAllAlbums))
              prop.disabled(
                match state.View with
                | Full _ -> true
                | _ -> false
              )
            ]
            Html.button [
              prop.text "Create New"
              prop.className "albums-sidebar-btn"
              prop.onClick(fun _ -> dispatch(SetView Create))
              prop.disabled(
                match state.View with
                | Create -> true
                | _ -> false
              )
            ]
          ]
        ]
        Html.main [
          prop.className "albums-main"
          prop.children [
            if state.IsLoading then
              Html.div [
                prop.className "albums-loading"
                prop.children [ Html.p "Loading..." ]
              ]
            match state.ErrorMessage with
            | Some err ->
              Html.div [ prop.className "albums-error"; prop.text err ]
            | None -> Html.none
            match state.View with
            | Full albums ->
              Html.section [
                Html.h3 [ prop.text "All Albums" ]
                if List.isEmpty albums then
                  Html.p [
                    prop.text "No albums found."
                    prop.className "albums-empty"
                  ]
                else
                  AlbumListView(
                    albums
                    |> List.map(fun album -> AlbumListItem(album, dispatch))
                  )
              ]
            | Popular albums ->
              Html.section [
                Html.h3 [ prop.text "Popular Albums" ]
                if List.isEmpty albums then
                  Html.p [
                    prop.text "No popular albums found."
                    prop.className "albums-empty"
                  ]
                else
                  AlbumListView(
                    albums
                    |> List.map(fun album -> AlbumListItem(album, dispatch))
                  )
              ]
            | Detail album ->
              Html.section [ albumDetailView album dispatch state ]
            | Update album ->
              Html.section [
                Html.h3 [ prop.text "Update Album" ]
                Form.AlbumForm(
                  Some album,
                  (fun args -> dispatch(RequestUpdateAlbum(args, album)))
                )
                Html.button [
                  prop.text "Cancel"
                  prop.className "albums-cancel-btn"
                  prop.onClick(fun _ -> dispatch(SetView(Detail album)))
                ]
              ]
            | Create ->
              Html.section [
                Html.h3 [ prop.text "Create Album" ]
                Form.AlbumForm(
                  None,
                  (fun args -> dispatch(RequestCreateAlbum args))
                )
                Html.button [
                  prop.text "Cancel"
                  prop.className "albums-cancel-btn"
                  prop.onClick(fun _ -> dispatch(SetView(Full state.Albums)))
                ]
              ]
            | NotFound msg -> Html.section [ notFoundView msg ]
          ]
        ]
      ]
    ]
