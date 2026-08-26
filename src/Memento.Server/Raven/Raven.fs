// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Server.Raven

open System.Threading
open Raven.Client.Documents

module Raven =
    let run (store: IDocumentStore) (ct: CancellationToken) (op: RavenOp<'T>) : Async<'T> =
        async {
            use session = store.OpenAsyncSession()

            let ctx =
                { Session = session
                  CancellationToken = ct }

            return! op ctx
        }

    let runWithSave (store: IDocumentStore) (ct: CancellationToken) (op: RavenOp<'T>) : Async<'T> =
        async {
            use session = store.OpenAsyncSession()

            let ctx =
                { Session = session
                  CancellationToken = ct }

            let! result = op ctx
            do! ctx.Session.SaveChangesAsync(ct)
            return result
        }
