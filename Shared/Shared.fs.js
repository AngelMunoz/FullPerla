import { Record } from "../Perla.Client/src/fable_modules/fable-library-js.4.25.0/Types.js";
import { record_type, int32_type, string_type, class_type } from "../Perla.Client/src/fable_modules/fable-library-js.4.25.0/Reflection.js";
import { list, datetime, guid, object } from "../Perla.Client/src/fable_modules/Thoth.Json.10.4.1/Encode.fs.js";
import { list as list_1, option, datetimeUtc, int, string, guid as guid_1, object as object_1 } from "../Perla.Client/src/fable_modules/Thoth.Json.10.4.1/Decode.fs.js";
import { uncurry2 } from "../Perla.Client/src/fable_modules/fable-library-js.4.25.0/Util.js";
import { map, delay, toList } from "../Perla.Client/src/fable_modules/fable-library-js.4.25.0/Seq.js";

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

export function Codecs_Album_encode(album) {
    return object([["id", guid(album.id)], ["title", album.title], ["artist", album.artist], ["rate", album.rate], ["releaseDate", datetime(album.releaseDate)]]);
}

export const Codecs_Album_decode = (path_4) => ((v) => object_1((get$) => {
    let objectArg, objectArg_1, objectArg_2, objectArg_3, objectArg_4;
    return new Album((objectArg = get$.Required, objectArg.Field("id", guid_1)), (objectArg_1 = get$.Required, objectArg_1.Field("title", string)), (objectArg_2 = get$.Required, objectArg_2.Field("artist", string)), (objectArg_3 = get$.Required, objectArg_3.Field("rate", uncurry2(int))), (objectArg_4 = get$.Required, objectArg_4.Field("releaseDate", datetimeUtc)));
}, path_4, v));

export const Codecs_Album_Option_decode = (path) => ((value) => option(uncurry2(Codecs_Album_decode), path, value));

export function Codecs_Albums_encode(albums) {
    return list(toList(delay(() => map(Codecs_Album_encode, albums))));
}

export const Codecs_Albums_decode = (path) => ((value) => list_1(uncurry2(Codecs_Album_decode), path, value));

export function Codecs_AlbumArgs_encode(args) {
    return object([["title", args.title], ["artist", args.artist], ["releaseDate", datetime(args.releaseDate)]]);
}

