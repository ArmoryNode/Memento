// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Server.Raven

module Session =
    let load<'T when 'T : not struct and 'T : not null> (id: string) : RavenOp<'T option> =
        fun ctx ->
            async {
                let! doc = ctx.Session.LoadAsync<'T>(id, ctx.CancellationToken)
                return Option.ofObj doc
            }

    let loadMany<'T when 'T : not struct and 'T : not null> (ids: string seq) : RavenOp<'T array> =
        fun ctx ->
            async {
                let! docs = ctx.Session.LoadAsync<'T>(ids, ctx.CancellationToken)

                return
                    docs.Values
                    |> Seq.choose Option.ofObj
                    |> Array.ofSeq
            }

    let store (entity: 'T) : RavenOp<unit> =
        fun ctx -> async { do! ctx.Session.StoreAsync(entity, ctx.CancellationToken) }

    let delete (id: string) : RavenOp<unit> =
        fun ctx -> async { ctx.Session.Delete(id) }

    let saveChanges: RavenOp<unit> =
        fun ctx -> async { do! ctx.Session.SaveChangesAsync(ctx.CancellationToken) }
