module InterviewTask.Api.ApiHandlers

open Giraffe
open Giraffe.EndpointRouting
open Giraffe.OpenApi
open Microsoft.AspNetCore.Http

type EchoRequest = {
  name: string
  numbers: int list
}

type EchoResponse = {
  greeting: string
  sum: int
  count: int
}

let private echoHandler: HttpHandler =
  fun (next: HttpFunc) (ctx: HttpContext) -> task {
    let! req = ctx.BindJsonAsync<EchoRequest>()

    let sum = req.numbers |> List.sum

    let res = {
      greeting = $"Hello, {req.name}!"
      sum = sum
      count = req.numbers.Length
    }

    return! json res next ctx
  }

let endpoints = [
  POST [
    route "/api/demo/echo" echoHandler
    |> configureEndpoint _.WithSummary("Echo demo (contract-first integration example)")
    |> addOpenApiSimple<EchoRequest, EchoResponse>
  ]
]
