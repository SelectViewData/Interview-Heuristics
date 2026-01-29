# InterviewTask

Welcome! This is a small full-stack project we use during the technical interview.

## Before the interview

Please make sure you can **build**, **run**, and **test** the project locally.

No implementation work is expected in advance; we will provide tasks during the interview.

### Prerequisites

- .NET SDK 10 (`dotnet --version` should show `10.x`)
- Node.js 20+ and npm
- (Optional) VS Code + Ionide for F#

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

We will work together on a few small tasks. Expect a mix of:

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
