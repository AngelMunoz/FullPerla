module Main

open Feliz
open App
open Browser.Dom

let root = ReactDOM.createRoot (document.getElementById "feliz-app")
let musicStore = Albums.CreateService()
root.render (Components.Router(musicStore))
