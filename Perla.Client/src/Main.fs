module Main

open Feliz
open Browser.Dom
open App.Services
open Perla.Shared

let root = ReactDOM.createRoot(document.getElementById "feliz-app")
let audioStore : AudioStore = AudioStore.CreateService()
let musicStore : MusicStore = MusicStore.CreateService()
root.render(App.ClientRouter.App(audioStore, musicStore))
