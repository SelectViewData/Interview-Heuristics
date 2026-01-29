namespace Giraffe.OpenApi

// custom version of Giraffe.OpenApi. Should be removed when Giraffe.OpenApi is updated to support swashbuckle v10

open Giraffe.OpenApi
open Microsoft.OpenApi
open Swashbuckle.AspNetCore.SwaggerGen

type RoutefOperationFilter() =
    interface IOperationFilter with
        member _.Apply(operation: OpenApiOperation, context: OperationFilterContext) =
            let metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata

            let routeMeta =
                metadata
                |> Seq.tryPick (function
                    | :? RoutefOpenApiParameters as r -> Some r
                    | _ -> None)

            match routeMeta with
            | None -> () // not a routef endpoint, leave as-is
            | Some r ->
                // 🚨 Important part:
                // When this metadata exists, we assume the route parameters are
                // *completely* defined by `routef`, so we overwrite whatever
                // Swashbuckle inferred (like `_arg1` query param).
                operation.Parameters <- (r.Parameters |> Seq.cast<IOpenApiParameter> |> ResizeArray)