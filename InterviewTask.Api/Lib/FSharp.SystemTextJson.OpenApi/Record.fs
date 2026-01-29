module internal FSharp.SystemTextJson.OpenApi.Record

open System
open System.Reflection
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.FSharp.Reflection
open Swashbuckle.AspNetCore.SwaggerGen

type internal PropertyDescription =
    { name: string
      propertyType: Type
      canBeSkipped: bool
      isNullable: bool
      info: PropertyInfo }

let private allProperties (fsOptions: JsonFSharpOptions) (recordType: Type) =
    let fields = FSharpType.GetRecordFields(recordType, true)

    let allPublic =
        recordType.GetProperties(BindingFlags.Instance ||| BindingFlags.Public)


    let all =
        if (nonNull (fields[0].GetGetMethod(true))).IsPublic then
            allPublic
        else
            Array.append fields allPublic

    if fsOptions.IncludeRecordProperties then
        all
    else
        all
        |> Array.filter (fun p ->
            Array.contains p fields
            || (p.GetCustomAttributes(typeof<JsonIncludeAttribute>, true) |> Seq.isEmpty |> not))

let private allProps (fsOptions: JsonFSharpOptions) (options: JsonSerializerOptions) (recordType: Type) =
    allProperties fsOptions recordType
    |> Seq.filter (fun p -> p.GetCustomAttributes(typeof<JsonIgnoreAttribute>, true) |> Array.isEmpty)
    |> Seq.map (fun p ->
        let names =
            match Helper.getJsonNames "field" (fun ty -> p.GetCustomAttributes(ty, true)) with
            | ValueSome names -> names |> Array.map (fun n -> n.AsString())
            | ValueNone -> [| Helper.convertName options.PropertyNamingPolicy p.Name |]

        let canBeSkipped =
            Helper.ignoreNullValues options || Helper.isSkippableType p.PropertyType

        let isNullable = Helper.isNullable p.PropertyType

        { name = names[0]
          canBeSkipped = canBeSkipped
          propertyType = p.PropertyType
          isNullable = isNullable
          info = p })

let getRecordDataProperties (``type``: Type) (fsOptions: JsonFSharpOptions) (options: JsonSerializerOptions) =
  let properties = allProps fsOptions options ``type``

  properties
  |> Seq.map (fun p ->
      DataProperty(
          p.name,
          p.propertyType,
          isRequired = not p.canBeSkipped,
          isNullable = p.isNullable,
          memberInfo = p.info
      ))

let createDataContract (``type``: Type) (fsOptions: JsonFSharpOptions) (options: JsonSerializerOptions) =
    let dataProperties = getRecordDataProperties ``type`` fsOptions options
    DataContract.ForObject(``type``, dataProperties, jsonConverter = Helper.getJsonConverterFunc options)