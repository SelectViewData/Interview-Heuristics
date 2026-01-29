module internal FSharp.SystemTextJson.OpenApi.Tuple

open System
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.FSharp.Reflection
open Microsoft.OpenApi
open Swashbuckle.AspNetCore.SwaggerGen

let createDataContractTuple (typeToConvert: Type) (options: JsonSerializerOptions) =
    let listType = AbstractSubtypes.abstractTupleType.MakeGenericType(typeToConvert)
    DataContract.ForArray(typeToConvert, listType, Helper.getJsonConverterFunc options)


type TupleSchemaFilter() =

    let getReferencedSchema (repo: SchemaRepository) (schema: IOpenApiSchema) =
        match schema with
        | :? OpenApiSchemaReference as reference when not (isNull reference.Target) ->
            reference.Target
        | :? OpenApiSchemaReference as reference when
            reference.Reference.Id |> String.IsNullOrEmpty |> not
            && repo.Schemas.ContainsKey(reference.Reference.Id)
            ->
            repo.Schemas[reference.Reference.Id]
        | _ -> schema

    interface ISchemaFilter with
        member this.Apply(schema, context) =
            match schema with
            | :? OpenApiSchema as schema ->
                if TypeCache.getKind context.Type = TypeCache.TypeKind.Tuple then
                    let cnt = context.Type |> FSharpType.GetTupleElements |> Array.length
                    schema.MaxItems <- cnt
                    schema.MinItems <- cnt

                let checkNullableIsEmpty =
                  Option.ofObj >> Option.map Seq.isEmpty >> Option.defaultValue true  
                
                match schema.OneOf with
                | NonNull oneOf when
                  context.Type.IsGenericType
                  && context.Type.GetGenericTypeDefinition() = AbstractSubtypes.abstractTupleType
                  && oneOf
                     |> Seq.map (getReferencedSchema context.SchemaRepository)
                     |> Seq.choose Option.ofObj
                     |> Seq.exists (fun c -> c.Type = Nullable(JsonSchemaType.String) && c.Enum |> checkNullableIsEmpty)
                  && oneOf
                     |> Seq.map (getReferencedSchema context.SchemaRepository)
                     |> Seq.choose Option.ofObj
                     |> Seq.exists (fun c -> c.Type = Nullable(JsonSchemaType.String) && not (c.Enum |> checkNullableIsEmpty)) 
                    ->
                    schema.AnyOf <- schema.OneOf
                    schema.OneOf <- System.Collections.Generic.List<_>()
                | _ -> ()
                
                if context.Type = typedefof<EmptyArray> then
                    schema.MaxItems <- 0
                    schema.MinItems <- 0
            | _ -> ()