// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Server.Raven

open System
open System.Threading
open Raven.Client.Documents.Session

type RavenContext =
    { Session: IAsyncDocumentSession
      CancellationToken: CancellationToken }

type RavenOp<'T> = RavenContext -> Async<'T>

module RavenOp =
    let ret value : RavenOp<'T> = fun _ -> async { return value }

    let bind (continuation: 'T -> RavenOp<'U>) (op: RavenOp<'T>) : RavenOp<'U> =
        fun ctx ->
            async {
                let! value = op ctx
                return! continuation value ctx
            }

    let map (mapping: 'T -> 'U) (op: RavenOp<'T>) : RavenOp<'U> =
        fun ctx ->
            async {
                let! value = op ctx
                return mapping value
            }

    let combine (first: RavenOp<'T>) (second: RavenOp<'T>) : RavenOp<'T> = bind (fun _ -> second) first

    let delay (generator: unit -> RavenOp<'T>) : RavenOp<'T> =
        fun ctx -> async.Delay(fun () -> generator () ctx)

    let using (resource: IDisposable) (body: IDisposable -> RavenOp<'T>) : RavenOp<'T> =
        fun ctx ->
            async {
                use res = resource
                return! body res ctx
            }

    let catch (op: RavenOp<'T>) (handler: exn -> RavenOp<'T>) : RavenOp<'T> =
        fun ctx ->
            async {
                try
                    return! op ctx
                with ex ->
                    return! handler ex ctx
            }

    let forLoop (sequence: seq<'T>) (body: 'T -> RavenOp<unit>) : RavenOp<unit> =
        fun ctx ->
            async {
                for item in sequence do
                    return! body item ctx
            }
    
    let ofAsync (asyncOp: Async<'T>) : RavenOp<'T> = fun _ -> asyncOp
   

type RavenBuilder() =
    member _.Return value = RavenOp.ret value
    member _.ReturnFrom(op: RavenOp<'T>) = op
    member _.Bind(op, continuation) = RavenOp.bind continuation op
    member _.Zero() = RavenOp.ret ()
    member _.Combine(first, second) = RavenOp.combine first second
    member _.Delay(generator) = RavenOp.delay generator
    member _.Using(resource: IDisposable, body: IDisposable -> RavenOp<'T>) : RavenOp<'T> = RavenOp.using resource body
    member _.TryWith(op: RavenOp<'T>, handler: exn -> RavenOp<'T>) : RavenOp<'T> = RavenOp.catch op handler
    member _.For(sequence: seq<'T>, body: 'T -> RavenOp<unit>) : RavenOp<unit> = RavenOp.forLoop sequence body
    member _.Run(op: RavenOp<'T>) = op

[<AutoOpen>]
module Builders =
    let raven = RavenBuilder()
