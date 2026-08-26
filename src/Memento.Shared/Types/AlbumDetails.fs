// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Shared.Types

open System

type AlbumDetails =
    { AlbumId: string
      AlbumName: string
      AlbumDate: DateOnly
      Photos: PhotoInfo array }
