import { Outlet, createRootRoute } from "@tanstack/react-router";
import { TanStackRouterDevtoolsPanel } from "@tanstack/react-router-devtools";
import { TanStackDevtools } from "@tanstack/react-devtools";

import { Link } from "@tanstack/react-router";
import { cn } from "@/lib/utils";

export const Route = createRootRoute({
  component: () => (
    <div className="min-h-screen">
      <header className="border-b">
        <div className="mx-auto flex max-w-5xl items-center gap-2 px-6 py-4">
          <div className="mr-3 text-sm font-semibold tracking-tight">
            InterviewTask
          </div>

          <Link
            to="/"
            className={cn(
              "rounded-md px-3 py-2 text-sm text-muted-foreground hover:bg-muted hover:text-foreground",
            )}
            activeProps={{ className: "bg-muted text-foreground" }}
          >
            Demo
          </Link>
        </div>
      </header>

      <main className="mx-auto max-w-5xl px-6 py-8">
        <Outlet />
      </main>

      <TanStackDevtools
        config={{ position: "bottom-right" }}
        plugins={[
          {
            name: "TanStack Router",
            render: <TanStackRouterDevtoolsPanel />,
          },
        ]}
      />
    </div>
  ),
});
