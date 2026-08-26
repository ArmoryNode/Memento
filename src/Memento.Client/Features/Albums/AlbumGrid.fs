// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Client.Features.Albums

open Bolero
open Bolero.Html
open Memento.Shared.Types
open Memento.Client.Templates

type AlbumGrid() =
    inherit ElmishComponent<AlbumListItem seq, AlbumMessage>()

    override this.View albums dispatch =
        div {
            attr.``class`` "album-card-grid"

            forEach albums
            <| fun album ->
                AlbumTemplates
                    .AlbumCard()
                    .Id(album.AlbumId)
                    .Name(album.Name)
                    .Date(album.Date.ToString("MMMM dd, yyyy"))
                    .CoverPhotoAltText(album.Name)
                    .Elt()
        }
