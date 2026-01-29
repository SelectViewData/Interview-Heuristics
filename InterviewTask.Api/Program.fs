open Giraffe
open Giraffe.EndpointRouting
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open InterviewTask.Api
open System.Text.Json.Serialization

[<EntryPoint>]
let main args =
  let builder = WebApplication.CreateBuilder(args)

  builder.Services.AddCors(fun options ->
    options.AddDefaultPolicy(fun policy ->
      policy
        .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173", "http://[::1]:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
      |> ignore))
  |> ignore

  builder.Services
    .AddRouting()
    .AddGiraffe()
    .AddEndpointsApiExplorer()
    .AddGiraffeOpenApi()
  |> ignore

  builder.Services.ConfigureHttpJsonOptions(fun options ->
    fsharpJsonOptions.AddToJsonSerializerOptions(options.SerializerOptions))
  |> ignore

  let app = builder.Build()

  app.UseCors().UseRouting() |> ignore

  app.UseGiraffe ApiHandlers.endpoints |> ignore

  if app.Environment.IsDevelopment() then
    app.MapScalarSwaggerOpenApi()

  app.Run()

  0 // Exit code
