module App.ClientRouter

open Feliz
open Feliz.Router
open Perla.Shared

open App.Routing
open App.Layout
open App.Pages

[<ReactComponent>]
let App(audioStore: AudioStore, musicStore: MusicStore) =
  let page = Router.currentPath() |> parseUrl

  let currentPage =
    match page with
    | Page.Player -> MusicPlayer.View(audioStore)
    | Page.AlbumsShowcase -> Albums.View.AlbumsShowcase(musicStore)
    | Page.NotFound -> Html.h1 "Not found"

  Html.div [
    prop.className "page-container"
    prop.children [
      Toolbar()
      React.router [
        router.onUrlChanged (fun _ -> ())
        router.children currentPage
      ]
    ]
  ]
