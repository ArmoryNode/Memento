// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Client.Features.Photos

open System
open System.IO
open Bolero
open Bolero.Html
open Memento.Client.Templates
open Memento.Shared.Constants
open Memento.Shared.Types
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Forms
open Microsoft.JSInterop

type PhotoUploadModal() =
    inherit ElmishComponent<PhotoUploadData, PhotoUploadMessage>()

    let fileInputRef = Ref<InputFile>()

    [<Inject>]
    member val JsRuntime: IJSRuntime = Unchecked.defaultof<_> with get, set

    override _.CssScope = CssScopes.PhotoUploadModal

    override this.View model dispatch =
        UtilityTemplates
            .Modal()
            .ModalTitle(
                div {
                    i { attr.``class`` "fa-utility-fill fa-semibold fa-image fa-lg" }
                    "\u0020Add Photo"
                }
            )
            .ModalContent(
                concat {
                    cond model.PreviewLoading
                    <| function
                        | true -> UtilityTemplates.Spinner().Message("Loading preview...").Elt()
                        | false -> empty ()

                    cond model.Uploading
                    <| function
                        | true -> UtilityTemplates.Spinner().Message("Uploading photo...").Elt()
                        | false -> empty ()

                    AlbumTemplates
                        .PhotoUploadForm()
                        .PhotoInput(
                            comp<InputFile> {
                                let inputId = "photoInput"

                                attr.id inputId
                                attr.name "photoUpload"
                                attr.``class`` "photo-input"
                                attr.accept SUPPORTED_IMAGE_TYPES

                                if model.Uploading then
                                    attr.disabled "disabled"
                                else
                                    attr.empty ()

                                attr.task.callback "OnChange" (fun (e: InputFileChangeEventArgs) ->
                                    task {
                                        let file = e.File
                                        use stream = file.OpenReadStream(file.Size)
                                        use ms = new MemoryStream()
                                        do! stream.CopyToAsync ms

                                        let uploadFile =
                                            { Name = file.Name
                                              ContentType = file.ContentType
                                              Data = ms.ToArray() }

                                        dispatch (SelectPhoto(uploadFile, None))
                                    })

                                fileInputRef
                            }
                        )
                        .PhotoDropzone(
                            cond model.File
                            <| function
                                | Some file ->
                                    AlbumTemplates
                                        .PhotoUploadContent()
                                        .PhotoPreviewUrl(model.PreviewUrl |> Option.defaultValue String.Empty)
                                        .PhotoName(file.Name, dispatch << UpdateName)
                                        .ClearSelectedPhoto(fun _ -> dispatch ClearPhoto)
                                        .Elt()
                                | None -> AlbumTemplates.PhotoDropzone().Elt()
                        )
                        .EnableUpload(
                            if model.File.IsSome && not model.Uploading then
                                null
                            else
                                "disabled"
                        )
                        .SaveAndUpload(fun _ ->
                            if not model.Uploading then
                                dispatch UploadPhoto)
                        .ClosePhotoUpload(fun _ ->
                            if not model.Uploading then
                                dispatch EndPhotoUpload)
                        .Elt()
                }
            )
            .Width("450px")
            .Height("250px")
            .CloseModal(fun _ ->
                if not model.Uploading then
                    dispatch EndPhotoUpload)
            .Elt()
