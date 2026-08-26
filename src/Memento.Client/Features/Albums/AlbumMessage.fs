// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Client.Features.Albums

open Memento.Client.Features.Photos
open Memento.Shared.Types

type AlbumMessage =
    | GetAlbums
    | GotAlbums of AlbumListItem array
    | GetAlbum of string
    | GotAlbum of AlbumDetails option
    | PhotoUploadMessage of PhotoUploadMessage
