module App.Layout

open Feliz

[<ReactComponent>]
let Toolbar() =
  Html.div [
    prop.className "toolbar"
    prop.children [
  Html.a [ prop.className "toolbar-link"; prop.href "/"; prop.text "Player" ]
  Html.a [ prop.className "toolbar-link"; prop.href "/albums"; prop.text "Albums" ]
    ]
  ]
