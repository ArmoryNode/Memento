// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Client.Features.Albums

open Bolero.Remoting
open Memento.Shared.Types

type AlbumService =
    {
        getAlbums: unit -> Async<AlbumListItem array>
        getAlbum: string -> Async<AlbumDetails option>
    }

    interface IRemoteService with
        member this.BasePath = "/albums"