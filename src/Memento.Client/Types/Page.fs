// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Client.Types

open Bolero

type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/album/{id}">] Album of id: string