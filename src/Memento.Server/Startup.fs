// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Server

open Bolero.Templating.Server
open Memento.Server.Services
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Bolero.Remoting.Server
open Bolero.Server
open Raven.DependencyInjection

type Startup() =
    member this.ConfigureServices(services: IServiceCollection) =
        services.AddMvc() |> ignore
        services.AddHttpClient() |> ignore
        services.AddServerSideBlazor() |> ignore
        services.AddBoleroRemoting<AlbumService>() |> ignore
        services.AddRavenDbDocStore() |> ignore
        services.AddRavenDbAsyncSession() |> ignore
        services
            .AddBoleroHost()
#if DEBUG
            .AddHotReload(templateDir = __SOURCE_DIRECTORY__ + "/../Memento.Client")
            .AddSassCompiler()
#endif
        |> ignore

    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if env.IsDevelopment() then
            app.UseWebAssemblyDebugging()

        app
            .UseAuthentication()
            .UseStaticFiles()
            .UseRouting()
            .UseAuthorization()
            .UseBlazorFrameworkFiles()
            .UseEndpoints(fun endpoints ->
#if DEBUG
                endpoints.UseHotReload()
#endif
                endpoints.MapBoleroRemoting() |> ignore
                endpoints.MapBlazorHub() |> ignore
                endpoints.MapFallbackToBolero(Index.page) |> ignore
                endpoints.MapControllers() |> ignore)
        |> ignore

module Program =

    [<EntryPoint>]
    let main args =
        Host
            .CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun webBuilder ->
                webBuilder.UseStaticWebAssets().UseStartup<Startup>() |> ignore)
            .Build()
            .Run()
        0
