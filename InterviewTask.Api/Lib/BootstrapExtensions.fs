[<AutoOpen>]
module InterviewTask.Api.BootstrapExtensions

open System.Text.Json.Serialization
open FSharp.SystemTextJson
open FSharp.SystemTextJson.OpenApi
open Giraffe
open Giraffe.OpenApi
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Routing
open Microsoft.Extensions.DependencyInjection
open Microsoft.OpenApi
open Scalar.AspNetCore

let fsharpJsonOptions =
  JsonFSharpOptions
    .Default()
    .WithUnionNamedFields()
    .WithUnionFieldNamesFromTypes()
    .WithUnionTagName("kind")
    .WithUnionInternalTag()
    .WithUnionUnwrapRecordCases()

type IServiceCollection with
  member this.AddGiraffeOpenApi(?options: JsonFSharpOptions) =
    let options = options |> Option.defaultValue fsharpJsonOptions

    this
      .AddSwaggerForSystemTextJson(options, fun opts ->
        opts.CustomSchemaIds(fun t ->
          let fullName =
            if isNull t.FullName then
              t.Name
            else
              t.FullName

          fullName
            .Replace("+", ".")
            .Replace(".", "_")
            .Replace("`", "_")
            .Replace("[", "_")
            .Replace("]", "_")
            .Replace(",", "_"))
        opts.OperationFilter<RoutefOperationFilter>()
        opts.OperationFilter<ConfigureOperationFilter>())
      .AddSingleton(typeof<Json.ISerializer>, Json.FsharpFriendlySerializer(options, Json.Serializer.DefaultOptions))

type IEndpointRouteBuilder with

  member this.MapScalarSwaggerOpenApi(?path: string) =
    this.MapSwagger(
      path |> Option.defaultValue "/openapi/{documentName}.json",
      fun x -> x.OpenApiVersion <- OpenApiSpecVersion.OpenApi3_1
    )
    |> ignore

    this.MapScalarApiReference() |> ignore
