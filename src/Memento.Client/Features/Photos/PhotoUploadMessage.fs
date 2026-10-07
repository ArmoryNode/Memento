// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Client.Features.Photos

open Memento.Shared.Types

type GeneratedThumbnail =
    { PreviewUrl: string option
      Thumbnail: UploadFile option }

type PhotoUploadMessage =
    | BeginPhotoUpload
    | GeneratedPreview of GeneratedThumbnail
    | SelectPhoto of UploadFile * string option
    | ClearPhoto
    | UpdateName of string
    | UploadPhoto
    | PhotoUploaded
    | EndPhotoUpload
    | UploadError of exn
