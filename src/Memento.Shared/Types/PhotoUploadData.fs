// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Shared.Types

open System

type UploadFile =
    { Name: string
      ContentType: string
      Data: byte[] }

[<Struct>]
type PhotoUploadData =
    { Name: string
      AltText: string
      PreviewUrl: string option
      File: UploadFile option
      PreviewLoading: bool
      Uploading: bool }
    
    static member Empty =
        { Name = String.Empty
          AltText = String.Empty
          PreviewUrl = None
          File = None
          PreviewLoading = false
          Uploading = false }
