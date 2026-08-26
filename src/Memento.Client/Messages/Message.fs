// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Client.Messages

open Memento.Client.Features.Albums
open Memento.Client.Types

type Message =
    | PageChanged of Page
    | AlbumMessage of AlbumMessage
    | Error of exn