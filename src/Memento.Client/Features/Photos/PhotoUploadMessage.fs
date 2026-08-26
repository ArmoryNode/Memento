// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Client.Features.Photos

open Memento.Shared.Types

type PhotoUploadMessage =
    | BeginPhotoUpload
    | GeneratedPreview of string option
    | SelectPhoto of UploadFile * string option
    | ClearPhoto
    | UpdateName of string
    | UploadPhoto
    | PhotoUploaded
    | EndPhotoUpload
    | UploadError of exn
