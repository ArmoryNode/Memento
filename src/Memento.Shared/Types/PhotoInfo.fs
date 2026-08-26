// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Shared.Types

[<Struct>]
type PhotoInfo =
    { PhotoUrl: string
      Width: int
      Height: int
      AltText: string
      SortOrder: int }
