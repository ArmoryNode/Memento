// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Server.Controllers

open System
open System.IO
open System.Threading
open System.Threading.Tasks
open Memento.Server.Models
open Memento.Server.Raven
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Raven.Client.Documents

[<Route("[controller]")>]
type ImagesController(store: IDocumentStore, env: IWebHostEnvironment) =
    inherit ControllerBase()

    member _.MissingPhotoImagePath =
        Path.Combine(env.ContentRootPath, "Assets/images/missing-image.png")

    [<ResponseCache(VaryByHeader = "User-Agent", Duration = 86400)>]
    [<HttpGet("MissingPhoto")>]
    member this.GetPlaceholderPhoto(ct: CancellationToken) =
        (File.OpenRead(this.MissingPhotoImagePath) :> Stream, "image/png")
        |> FileStreamResult

    [<ResponseCache(VaryByHeader = "User-Agent", Duration = 86400)>]
    [<HttpGet("{albumId}")>]
    member this.GetAlbumCover([<FromRoute>] albumId: string, ct: CancellationToken) =
        let op =
            raven {
                match! Session.load<Album> albumId with
                | None -> return None
                | Some album ->
                    let! attachment = Attachments.tryGetFor<Album> album Album.CoverPhotoFileName
                    return attachment
            }

        task {
            let! attachment = Raven.run store ct op

            let stream, contentType =
                match attachment with
                | Some attachment -> attachment.Stream, attachment.ContentType
                | None -> File.OpenRead(this.MissingPhotoImagePath) :> Stream, "image/png"

            return FileStreamResult(stream, contentType)
        }

    [<ResponseCache(VaryByHeader = "User-Agent", Duration = 86400)>]
    [<HttpGet("{albumId}/{photoName}")>]
    member this.GetAlbumPhoto([<FromRoute>] albumId: string, [<FromRoute>] photoName: string, ct: CancellationToken) =
        task {
            let! attachment = Attachments.tryGetForId albumId photoName |> Raven.run store ct

            let stream, contentType =
                match attachment with
                | None -> File.OpenRead(this.MissingPhotoImagePath) :> Stream, "image/png"
                | Some attachment -> attachment.Stream, attachment.ContentType

            return FileStreamResult(stream, contentType)
        }

    [<HttpPost("{albumId}")>]
    [<RequestSizeLimit(1_073_741_824L)>]
    [<RequestFormLimits(MultipartBodyLengthLimit = 1_073_741_824L)>]
    member this.UploadAlbumPhoto
        ([<FromRoute>] albumId: string, [<FromForm(Name = "photo")>] photo: IFormFile, ct: CancellationToken)
        =
        let op =
            raven {
                match! Session.load<Album> albumId with
                | None -> return this.NotFound() :> IActionResult
                | Some album ->
                    // Todo - Generate the image thumbnail, and store it along with the original photo
                    // as attachments in RavenDB
                    return this.Ok() :> IActionResult
            }

        task {
            do! Task.Delay(4000, ct)
            let! result = Raven.run store ct op |> Async.StartImmediateAsTask
            return result
        }
