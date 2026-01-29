import { useMemo, useState } from "react";

import { $api } from "@/api/client";
import type { paths } from "@/api/types";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

type EchoRequest =
  paths["/api/demo/echo"]["post"]["requestBody"]["content"]["application/json"];
type EchoResponse =
  paths["/api/demo/echo"]["post"]["responses"][200]["content"]["application/json"];

function parseNumbers(input: string) {
  return input
    .split(",")
    .map((x) => x.trim())
    .filter(Boolean)
    .map((x) => Number(x))
    .filter(Number.isFinite)
    .map((x) => Math.trunc(x));
}

export function DemoPage() {
  const mutation = $api.useMutation("post", "/api/demo/echo");

  const [name, setName] = useState("Ada");
  const [numbersText, setNumbersText] = useState("1, 2, 3");

  const request = useMemo<EchoRequest>(
    () => ({
      name,
      numbers: parseNumbers(numbersText),
    }),
    [name, numbersText],
  );

  const onSend = async () => {
    await mutation.mutateAsync({ body: request });
  };

  const result = mutation.data as EchoResponse | undefined;

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Demo</CardTitle>
          <CardDescription>
            A tiny end-to-end example: F# backend emits OpenAPI, frontend
            generates types and makes a typed request via `openapi-fetch` +
            `openapi-react-query`.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-3 md:grid-cols-2">
            <label className="space-y-1.5">
              <div className="text-xs font-medium text-muted-foreground">
                Name
              </div>
              <input
                className="h-9 w-full rounded-md border bg-background px-3 text-sm"
                value={name}
                onChange={(e) => setName(e.target.value)}
              />
            </label>

            <label className="space-y-1.5">
              <div className="text-xs font-medium text-muted-foreground">
                Numbers (comma-separated)
              </div>
              <input
                className="h-9 w-full rounded-md border bg-background px-3 text-sm"
                value={numbersText}
                onChange={(e) => setNumbersText(e.target.value)}
              />
            </label>
          </div>

          <div className="flex items-center gap-3">
            <Button onClick={onSend} disabled={mutation.isPending}>
              {mutation.isPending ? "Sending…" : "Send"}
            </Button>
            {mutation.isError && (
              <div className="text-sm text-destructive">
                {String(mutation.error as unknown)}
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Request</CardTitle>
          <CardDescription>
            Typed request payload (what gets POSTed).
          </CardDescription>
        </CardHeader>
        <CardContent>
          <pre className="rounded-md border bg-muted p-3 text-xs leading-relaxed">
            {JSON.stringify(request, null, 2)}
          </pre>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Response</CardTitle>
          <CardDescription>Typed response payload.</CardDescription>
        </CardHeader>
        <CardContent>
          <pre className="rounded-md border bg-muted p-3 text-xs leading-relaxed">
            {JSON.stringify(result ?? null, null, 2)}
          </pre>
        </CardContent>
      </Card>
    </div>
  );
}
