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

open Fable.Remoting.Server
open Fable.Remoting.AspNetCore

module Program =
    let exitCode = 0

    let getMusicStoreApp () =
        let store = Albums.CreateService()

        Remoting.createApi () |> Remoting.fromValue store


    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)


        let app = builder.Build()

        app.UseRemoting(getMusicStoreApp ())

        app.Run()

        exitCode
