# InterviewTask

Welcome! This is a small full-stack project we use during the technical interview.

## Before the interview

Please make sure you can **build**, **run**, and **test** the project locally.

No implementation work is expected in advance; we will provide tasks during the interview.

### Prerequisites

- .NET SDK 10 (`dotnet --version` should show `10.x`)
- Node.js 20+ and npm
- Claude Code CLI (`claude -v` should work)
- (Optional) VS Code + Ionide for F#

### Claude Code (Required)

You are welcome to use any other agentic coding CLI you prefer in addition to Claude Code.

During the interview, we may ask you to also use Claude Code on your machine. We will provide a temporary API key during the meeting, but please install the Claude Code CLI before the interview.

Install:

macOS / Linux / WSL:

```bash
curl -fsSL https://claude.ai/install.sh | bash
```

Windows:

```powershell
winget install Anthropic.ClaudeCode
```

Verify it works:

```bash
claude -v
```

During the interview, we will provide you with a temporary API key (so you don't incur any costs). Set it as an environment variable:

macOS / Linux / WSL (Bash/Zsh):

```bash
export ANTHROPIC_API_KEY="PASTE_KEY_HERE"
```

Windows (PowerShell):

```powershell
$env:ANTHROPIC_API_KEY="PASTE_KEY_HERE"
```

Then run:

```bash
claude
```

Inside Claude Code, run `/status` to confirm authentication is active.

### Quick Start (Terminal)

Install frontend dependencies:

```sh
cd interviewtask-web
npm install
```

Run the backend:

```sh
cd InterviewTask.Api
dotnet run --launch-profile Run
```

Run the frontend (in a second terminal):

```sh
cd interviewtask-web
npm run dev
```

Run tests:

```sh
cd InterviewTask.Tests
dotnet test
```

### What You Should See

- Frontend: `http://localhost:5173`
- API reference UI: `http://localhost:5080/scalar/v1`
- If the backend starts on a different port, use the URL printed in the terminal output.

## During the interview

We will work together on a few small tasks. You can use your agentic CLI during the session, but we will also ask you to do some steps manually (e.g., reading code, running commands, and making focused edits).

Expect a mix of:

- Explaining how a part of the code works and how you would approach changing it.
- Making a small backend change and validating it.
- Making a small UI change that calls the backend.

## VS Code (Optional)

1. Open VS Code at this folder (the one containing `InterviewTask.sln`).
2. Wait for Ionide to finish loading `InterviewTask.sln`.
3. `Run and Debug`:
   - `API (.NET)` to run the backend.
   - `Full stack (API + Web)` to run backend + frontend together.
4. Or use `Terminal -> Run Task...`:
   - `Dev: API + Web` (runs both)

## Technical notes (FYI)

You should not need this before the interview, but it can be helpful context:

- Backend: F#/.NET API with OpenAPI (browseable via Scalar).
- Frontend: React + TypeScript (Vite) using generated OpenAPI types.

If you hit a CORS error, make sure you open the frontend at `http://localhost:5173`.
