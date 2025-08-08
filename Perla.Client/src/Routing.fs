module App.Routing

/// Page discriminated union for typed routing
/// and a URL parser to map path segments to a page.
type Page =
  | Player
  | AlbumsShowcase
  | NotFound

/// Convert a list of URL segments into a Page DU
let parseUrl =
  function
  | [] -> Page.Player
  | [ "player" ] -> Page.Player
  | [ "albums" ] -> Page.AlbumsShowcase
  | _ -> Page.NotFound
