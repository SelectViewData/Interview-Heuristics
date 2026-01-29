namespace Giraffe.OpenApi

open Microsoft.OpenApi
open Swashbuckle.AspNetCore.SwaggerGen

type ConfigureOperationFilter() =
  interface IOperationFilter with
    member _.Apply(operation: OpenApiOperation, context: OperationFilterContext) =
      context.ApiDescription.ActionDescriptor.EndpointMetadata
      |> Seq.iter (function
        | :? ConfigureOperationMetadata as meta ->
          meta.Configure(operation, context)
        | _ -> ())
