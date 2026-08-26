// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Client.Models

open Memento.Client.Types
open Memento.Shared.Types

type AppModel =
    { CurrentPage: Page
      Albums: AlbumListItem array
      IsLoaded: bool
      SelectedAlbum: AlbumDetails option
      PhotoUpload: PhotoUpload }