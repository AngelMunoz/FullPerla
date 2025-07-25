import { Record } from "../Perla.Client/src/fable_modules/fable-library-js.4.25.0/Types.js";
import { option_type, lambda_type, list_type, unit_type, record_type, int32_type, string_type, class_type } from "../Perla.Client/src/fable_modules/fable-library-js.4.25.0/Reflection.js";

export class Album extends Record {
    constructor(id, title, artist, rate, releaseDate) {
        super();
        this.id = id;
        this.title = title;
        this.artist = artist;
        this.rate = (rate | 0);
        this.releaseDate = releaseDate;
    }
}

export function Album_$reflection() {
    return record_type("Perla.Shared.Album", [], Album, () => [["id", class_type("System.Guid")], ["title", string_type], ["artist", string_type], ["rate", int32_type], ["releaseDate", class_type("System.DateTime")]]);
}

export class NewAlbumArgs extends Record {
    constructor(title, artist, releaseDate) {
        super();
        this.title = title;
        this.artist = artist;
        this.releaseDate = releaseDate;
    }
}

export function NewAlbumArgs_$reflection() {
    return record_type("Perla.Shared.NewAlbumArgs", [], NewAlbumArgs, () => [["title", string_type], ["artist", string_type], ["releaseDate", class_type("System.DateTime")]]);
}

export class MusicStore extends Record {
    constructor(findPopular, findAll, findById, create, update) {
        super();
        this.findPopular = findPopular;
        this.findAll = findAll;
        this.findById = findById;
        this.create = create;
        this.update = update;
    }
}

export function MusicStore_$reflection() {
    return record_type("Perla.Shared.MusicStore", [], MusicStore, () => [["findPopular", lambda_type(unit_type, class_type("Microsoft.FSharp.Control.FSharpAsync`1", [list_type(Album_$reflection())]))], ["findAll", lambda_type(unit_type, class_type("Microsoft.FSharp.Control.FSharpAsync`1", [list_type(Album_$reflection())]))], ["findById", lambda_type(class_type("System.Guid"), class_type("Microsoft.FSharp.Control.FSharpAsync`1", [option_type(Album_$reflection())]))], ["create", lambda_type(NewAlbumArgs_$reflection(), class_type("Microsoft.FSharp.Control.FSharpAsync`1", [option_type(Album_$reflection())]))], ["update", lambda_type(Album_$reflection(), class_type("Microsoft.FSharp.Control.FSharpAsync`1", [option_type(Album_$reflection())]))]]);
}

