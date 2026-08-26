// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Server.Services

open Bolero.Remoting.Server
open Memento.Server.Models
open Memento.Server.Raven
open Memento.Shared.Types
open Raven.Client.Documents
open Memento.Client.Features
open Raven.Client.Documents.Linq

type AlbumService(ctx: IRemoteContext, store: IDocumentStore) =
    inherit RemoteHandler<Albums.AlbumService>()

    member this.Cancellation =
        match ctx.HttpContext with
        | null -> Async.DefaultCancellationToken
        | httpContext -> httpContext.RequestAborted

    override this.Handler =
        { getAlbums =
            fun () ->
                raven {
                    let! albumQuery = Query.from<Album>

                    let sorted =
                        query {
                            for album in albumQuery do
                                sortByDescending album.Date
                                select album
                        }

                    let! albums = Query.toArrayAsync (sorted :?> IRavenQueryable<_>)

                    return
                        albums
                        |> Array.map (fun album ->
                            { AlbumId = album.Id
                              Name = album.Name
                              Date = album.Date })
                }
                |> Raven.run store this.Cancellation
          getAlbum =
            fun albumId ->
                raven {
                    match! Session.load<Album> albumId with
                    | None -> return None
                    | Some album ->
                        return
                            Some
                                { AlbumId = album.Id
                                  AlbumName = album.Name
                                  AlbumDate = album.Date
                                  Photos = [||] }
                }
                |> Raven.run store this.Cancellation }
