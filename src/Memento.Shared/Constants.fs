// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

module Memento.Shared.Constants

let SITE_TITLE = "ArmoryNode"
let SUPPORTED_IMAGE_TYPES =
    [| "jpeg"; "jxl"; "png"; "webp"; "avif"; "heif"; "heic"; "tiff" |]
    |> Seq.map (sprintf "image/%s")
    |> String.concat ","