// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

module Memento.Client.Main

open System
open System.IO
open System.Net.Http
open Bolero.Remoting
open Bolero.Remoting.Client
open Bolero.Templating.Client
open Elmish
open Bolero
open Bolero.Html
open Memento.Client.Features.Photos
open Memento.Client.Features.Albums
open Memento.Client.Messages
open Memento.Client.Templates
open Memento.Client.Types
open Memento.Client.Models
open Memento.Shared.Types
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Forms
open Microsoft.JSInterop

let initModel =
    { CurrentPage = Home
      Albums = [||]
      SelectedAlbum = None
      PhotoUpload = Idle
      IsLoaded = false }

let uploadPhoto (http: HttpClient, albumId: string, file: UploadFile) =
    async {
        use formDataContent = new MultipartFormDataContent()
        let content = new ByteArrayContent(file.Data)
        content.Headers.ContentType <- System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType)
        formDataContent.Add(content, "photo", file.Name)

        let! response = http.PostAsync($"/Images/{albumId}", formDataContent)
        response.EnsureSuccessStatusCode() |> ignore
        printfn $"is success: %b{response.IsSuccessStatusCode}"
    }

let generatePhotoPreview (file: UploadFile, jsRuntime: IJSRuntime) =
    async {
        use stream = new MemoryStream(file.Data)
        use streamRef = new DotNetStreamReference(stream)
        let! preview = jsRuntime.InvokeAsync("photoUtils.getThumbnailUrl", streamRef, file.ContentType)
        printfn $"preview: %s{preview}"
        return preview |> Option.ofObj
    }

let updatePhotoUpload js http (selectedAlbum: AlbumDetails option) message photoUpload =
    match message with
    | SelectPhoto(file, previewUrl) ->
        let data =
            match photoUpload with
            | Uploading upload ->
                { upload with
                    File = Some file
                    PreviewUrl = previewUrl
                    PreviewLoading = true }
            | Idle ->
                { PhotoUploadData.Empty with
                    File = Some file
                    PreviewUrl = previewUrl
                    PreviewLoading = true }

        let onSuccess = GeneratedPreview >> PhotoUploadMessage >> AlbumMessage
        Uploading data, Cmd.OfAsync.perform generatePhotoPreview (file, js) onSuccess
    | GeneratedPreview previewUrl ->
        let photoUpload =
            photoUpload
            |> PhotoUpload.map (fun upload ->
                { upload with
                    PreviewUrl = previewUrl
                    PreviewLoading = false
                    Uploading = false })

        photoUpload, Cmd.none
    | ClearPhoto ->
        let photoUpload, cmd =
            match photoUpload with
            | Uploading upload ->
                let cmd =
                    match photoUpload with
                    | Uploading model ->
                        Cmd.OfJS.attempt js "photoUtils.revokePreviewUrl" [| model.PreviewUrl |> Option.toObj |] Error
                    | Idle -> Cmd.none

                Uploading
                    { upload with
                        PreviewUrl = None
                        File = None },
                cmd
            | _ -> Idle, Cmd.none

        photoUpload, cmd
    | UploadPhoto ->
        let nextModel =
            photoUpload |> PhotoUpload.map (fun upload -> { upload with Uploading = true })

        let cmd =
            match selectedAlbum, photoUpload |> PhotoUpload.get _.File with
            | Some album, Some photo ->
                let onComplete _ =
                    PhotoUploaded |> PhotoUploadMessage |> AlbumMessage

                let onError = UploadError >> PhotoUploadMessage >> AlbumMessage

                Cmd.OfAsync.either uploadPhoto (http, album.AlbumId, photo) onComplete onError
            | _ -> Cmd.none

        nextModel, cmd
    | PhotoUploaded ->
        let msg = EndPhotoUpload |> PhotoUploadMessage |> AlbumMessage

        let nextPhotoUpload =
            photoUpload |> PhotoUpload.map (fun upload -> { upload with Uploading = false })

        nextPhotoUpload, Cmd.ofMsg msg
    | EndPhotoUpload -> Idle, Cmd.ofMsg (ClearPhoto |> PhotoUploadMessage |> AlbumMessage)
    | UpdateName name ->
        let photoUpload =
            match photoUpload with
            | Uploading photoUpload -> { photoUpload with Name = name }
            | Idle ->
                { PhotoUploadData.Empty with
                    Name = name }

        Uploading photoUpload, Cmd.none
    | BeginPhotoUpload ->
        let photoUpload = PhotoUploadData.Empty
        Uploading photoUpload, Cmd.none
    | UploadError ex ->
        eprintf "An error has occurred during photo upload"

        let photoUpload =
            photoUpload |> PhotoUpload.map (fun upload -> { upload with Uploading = false })

        photoUpload, Cmd.ofMsg (Error ex)

let updateAlbum remote js http message model =
    match message with
    | GetAlbums ->
        let msg = GotAlbums >> AlbumMessage
        let cmd = Cmd.OfAsync.either remote.getAlbums () msg Error
        { model with IsLoaded = false }, cmd
    | GotAlbums albums ->
        { model with
            Albums = albums
            IsLoaded = true },
        Cmd.none
    | GetAlbum id ->
        let msg = GotAlbum >> AlbumMessage
        let cmd = Cmd.OfAsync.either remote.getAlbum id msg Error
        { model with IsLoaded = false }, cmd
    | GotAlbum details ->
        { model with
            IsLoaded = true
            SelectedAlbum = details },
        Cmd.none
    | PhotoUploadMessage message ->
        let photoUpload, cmd =
            updatePhotoUpload js http model.SelectedAlbum message model.PhotoUpload

        { model with PhotoUpload = photoUpload }, cmd

let update remote js http message model =
    match message with
    | PageChanged page ->
        let cmd =
            match page with
            | Album id -> Cmd.ofMsg (AlbumMessage(GetAlbum id))
            | _ -> Cmd.none

        { model with CurrentPage = page }, cmd
    | AlbumMessage message -> updateAlbum remote js http message model
    | Error ex ->
        eprintfn $"An error has occurred: %s{ex.Message}"
        model, Cmd.none

let view model dispatch =
    div {
        cond model.IsLoaded
        <| function
            | true ->
                cond model.CurrentPage
                <| function
                    | Home ->
                        let albumDispatch = dispatch << AlbumMessage
                        ecomp<AlbumGrid, _, _> model.Albums albumDispatch { attr.empty () }
                    | Album _ ->
                        let photoUploadDispatch = dispatch << AlbumMessage << PhotoUploadMessage

                        concat {
                            cond model.SelectedAlbum
                            <| function
                                | Some album -> ecomp<PhotoGrid, _, _> album photoUploadDispatch { attr.empty () }
                                | None -> empty ()

                            cond model.PhotoUpload
                            <| function
                                | Uploading upload ->
                                    ecomp<PhotoUploadModal, _, _> upload photoUploadDispatch { attr.empty () }
                                | Idle -> empty ()
                        }
            | false -> UtilityTemplates.Spinner().Elt()
    }

type App() =
    inherit ProgramComponent<AppModel, Message>()

    [<Inject>]
    member val JSInterop: IJSRuntime = Unchecked.defaultof<_> with get, set

    [<Inject>]
    member val HttpClientFactory: IHttpClientFactory = Unchecked.defaultof<_> with get, set

    override this.Program =
        let httpClient = this.HttpClientFactory.CreateClient()
        httpClient.BaseAddress <- this.NavigationManager.BaseUri |> Uri

        let albumService = this.Remote<AlbumService>()
        let update = update albumService this.JSInterop httpClient
        let initMessage = AlbumMessage <| GetAlbums

        Program.mkProgram (fun _ -> initModel, Cmd.ofMsg initMessage) update view
        |> Program.withRouterInfer PageChanged _.CurrentPage
#if DEBUG
        |> Program.withHotReload
#endif
