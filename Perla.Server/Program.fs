namespace Perla.Server

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

module Program =
  open Perla.Shared
  let exitCode = 0

  [<EntryPoint>]
  let main args =

    let builder = WebApplication.CreateBuilder(args)

    builder.Services.AddSingleton<MusicStore>(fun _ -> Albums.CreateService())
    |> ignore

    let app = builder.Build()

    Albums.RegisterRoutes app
    Music.RegisterRoutes app

    app.Run()

    exitCode
