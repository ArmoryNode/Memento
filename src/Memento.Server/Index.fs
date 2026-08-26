// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

module Memento.Server.Index

open Bolero.Html
open Bolero.Server.Html
open Memento
open Memento.Shared

let page = doctypeHtml {
    head {
        meta { attr.charset "UTF-8" }
        meta { attr.name "viewport"; attr.content "width=device-width, initial-scale=1.0" }
        title { Constants.SITE_TITLE }
        ``base`` { attr.href "/" }
        link { attr.rel "stylesheet"; attr.href "/Memento.Client.styles.css" }
        link { attr.rel "dns-prefetch"; attr.href "//use.typekit.net" }
        link { attr.rel "dns-prefetch"; attr.href "//kit.fontawesome.com" }
        link { attr.rel "stylesheet"; attr.href "//use.typekit.net/imt1wuo.css" }
        link { attr.rel "stylesheet"; attr.href "/css/main.css" }
        script { attr.src "https://kit.fontawesome.com/a413cbe63b.js"; attr.crossorigin "anonymous" }
    }
    header {
        h1 {
            a {
                attr.href "/"
                i { attr.``class`` "fa-solid fa-film-canister" }
                $"\u0020{Constants.SITE_TITLE}"
            }
        }
    }
    body {
        div { attr.id "main"; comp<Client.Main.App> }
        boleroScript
        script { attr.src "/js/photo-utilities.js" }
    }
}
