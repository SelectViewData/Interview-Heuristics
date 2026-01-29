namespace Giraffe.OpenApi

// custom version of Giraffe.OpenApi. Should be removed when Giraffe.OpenApi is updated to support swashbuckle v10

open System.Collections.Generic
open System.Reflection
open FSharp.Reflection

[<AutoOpen>]
module Routing =

    open Microsoft.AspNetCore.Builder
    open Microsoft.OpenApi
    open Giraffe
    open Giraffe.EndpointRouting

    /// Metadata we’ll attach to Giraffe endpoints so Swashbuckle can see route params
    type RoutefOpenApiParameters(parameters: IReadOnlyList<OpenApiParameter>) =
        member _.Parameters = parameters

    let private getSchema (c: char) (modifier: string option) =
        match c with
        | 's' -> OpenApiSchema(Type = JsonSchemaType.String)
        | 'i' -> OpenApiSchema(Type = JsonSchemaType.Integer, Format = "int32")
        | 'b' -> OpenApiSchema(Type = JsonSchemaType.Boolean)
        | 'c' -> OpenApiSchema(Type = JsonSchemaType.String)
        | 'd' -> OpenApiSchema(Type = JsonSchemaType.Integer, Format = "int64")
        | 'f' -> OpenApiSchema(Type = JsonSchemaType.Number, Format = "double")
        | 'u' -> OpenApiSchema(Type = JsonSchemaType.Integer, Format = "int64")
        | 'O' ->
            match modifier with
            | Some "guid" -> OpenApiSchema(Type = JsonSchemaType.String, Format = "uuid")
            | _ -> OpenApiSchema(Type = JsonSchemaType.String)
        | _ -> OpenApiSchema(Type = JsonSchemaType.String)

    let routef (path: PrintfFormat<_, _, _, _, 'T>) (routeHandler: 'T -> HttpHandler) : Endpoint =
        let template, mappings = RouteTemplateBuilder.convertToRouteTemplate path
        let boxedHandler (o: obj) =
            let t = o :?> 'T
            routeHandler t

        let configureEndpoint =
            fun (endpoint: IEndpointConventionBuilder) ->
                let parameters =
                    mappings
                    |> Seq.map (fun (name, format) ->
                        OpenApiParameter(
                            Name = name,
                            In = ParameterLocation.Path,
                            Required = true,
                            Style = ParameterStyle.Simple,
                            Schema = getSchema format None   // <-- reuse your existing logic
                        )
                    )
                    |> ResizeArray

                endpoint.AddOpenApiOperationTransformer(fun operation context ct ->
                    operation.Parameters <- parameters |> Seq.cast<IOpenApiParameter> |> ResizeArray
                    System.Threading.Tasks.Task.CompletedTask
                ) |> ignore
                
                // Attach as endpoint metadata, for Swashbuckle to pick up later
                endpoint.WithMetadata(RoutefOpenApiParameters(parameters))
        
        TemplateEndpoint(HttpVerb.NotSpecified, template, mappings, boxedHandler, configureEndpoint)

    let addOpenApi (config: OpenApiConfig) = configureEndpoint config.Build

    let addOpenApiSimple<'Req, 'Res> =
        let methodName =
            match typeof<'Req>, typeof<'Res> with
            | reqType, respType when reqType = unitType && respType = unitType -> "InvokeUnit"
            | reqType, _ when reqType = unitType -> "InvokeUnitReq"
            | _, respType when respType = unitType -> "InvokeUnitResp"
            | reqType, _ when FSharpType.IsTuple reqType -> "InvokeUnitReq"
            | _ -> "Invoke"
            
        let requestBody =
            if typeof<'Req> = unitType then
                None
            else
                Some(RequestBody(requestType = typeof<'Req>))
        
        let configure x =
          x.WithMetadata(
                typeof<FakeFunc<'Req, 'Res>>
                    .GetMethod(methodName, BindingFlags.Instance ||| BindingFlags.NonPublic)
                |> nullArgCheck $"Method {methodName} not found"
            ) |> ignore
          requestBody
          |> Option.iter (fun accepts -> x.WithMetadata(accepts.ToAttribute()) |> ignore)
          x
          
        configureEndpoint configure