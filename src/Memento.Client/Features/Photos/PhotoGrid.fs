// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Client.Features.Photos

open Bolero
open Bolero.Html
open Memento.Shared.Types
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open Memento.Client.Templates

type PhotoGrid() =
    inherit ElmishComponent<AlbumDetails, PhotoUploadMessage>()

    [<Inject>]
    member val IJSRuntime: IJSRuntime = null with get, set

    override this.View model dispatch =
        AlbumTemplates
            .AlbumDetails()
            .PhotoList(
                forEach model.Photos
                <| fun info ->
                    AlbumTemplates
                        .AlbumPhoto()
                        .PhotoUrl(info.PhotoUrl)
                        .AltText(info.AltText)
                        .Width(info.Width)
                        .Height(info.Height)
                        .Elt()
            )
            .UploadPhoto(fun _ -> dispatch (BeginPhotoUpload))
            .Elt()
