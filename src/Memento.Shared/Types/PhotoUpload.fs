// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Shared.Types

type PhotoUpload =
    | Uploading of PhotoUploadData
    | Idle

module PhotoUpload =        
    let inline map ([<InlineIfLambda>] mapping) photoUpload =
        match photoUpload with
        | Idle -> Idle
        | Uploading u -> Uploading (mapping u)
        
    let inline get ([<InlineIfLambda>] binder) photoUpload =
        match photoUpload with
        | Idle -> None
        | Uploading u -> binder u
