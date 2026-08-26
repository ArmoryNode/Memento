// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Server.Models

open System

type Album =
    { mutable Id: string
      Name: string
      Date: DateOnly }

    static member CoverPhotoFileName = "cover-photo.png"