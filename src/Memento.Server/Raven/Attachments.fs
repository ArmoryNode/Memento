// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Server.Raven

open System.IO

module Attachments =
    type AttachmentData = { Stream: Stream; ContentType: string }

    let tryGetFor<'T> (entity: 'T) name : RavenOp<AttachmentData option> =
        fun ctx ->
            async {
                let! result = ctx.Session.Advanced.Attachments.GetAsync(entity, name, ctx.CancellationToken)

                return
                    result
                    |> Option.ofObj
                    |> Option.map (fun att ->
                        { Stream = att.Stream
                          ContentType = att.Details.ContentType })
            }

    let tryGetById id name : RavenOp<AttachmentData option> =
        fun ctx ->
            async {
                let! result = ctx.Session.Advanced.Attachments.GetAsync(id, name, ctx.CancellationToken)

                return
                    result
                    |> Option.ofObj
                    |> Option.map (fun att ->
                        { Stream = att.Stream
                          ContentType = att.Details.ContentType })
            }

    let storeById id name data : RavenOp<unit> =
        fun ctx -> async { ctx.Session.Advanced.Attachments.Store(id, name, data.Stream, data.ContentType) }

    let store<'T> (entity: 'T) name data : RavenOp<unit> =
        fun ctx -> async { ctx.Session.Advanced.Attachments.Store(entity, name, data.Stream, data.ContentType) }

    let tryGetForId<'T when 'T: not struct and 'T: not null> id name : RavenOp<AttachmentData option> =
        raven {
            match! Session.load<'T> id with
            | None -> return None
            | Some entity -> return! tryGetFor entity name
        }
