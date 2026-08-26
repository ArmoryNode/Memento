// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Server.Raven

open Raven.Client.Documents
open Raven.Client.Documents.Linq

module Query =
    let from<'T> : RavenOp<IRavenQueryable<'T>> =
        fun ctx -> async { return ctx.Session.Query<'T>() }

    let toArrayAsync (query: IRavenQueryable<'T>) : RavenOp<'T array> =
        fun ctx -> async { return! query.ToArrayAsync(ctx.CancellationToken) }

    let firstOrNone<'T when 'T: not struct and 'T: not null> (query: IRavenQueryable<'T>) : RavenOp<'T option> =
        fun ctx ->
            async {
                let! result = query.FirstOrDefaultAsync(ctx.CancellationToken)
                return Option.ofObj result
            }
