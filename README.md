# Swimming Coach

A personal AI-assisted endurance coaching application focused initially on swimming.

## Project structure

- `src/SwimmingCoach.Api` — ASP.NET Core backend
- `src/swimming-coach-web` — React and TypeScript frontend
- `tests/SwimmingCoach.Api.Tests` — backend tests
- `PRODUCT_SPEC.md` — product requirements
- `BUILD_PLAN.md` — incremental implementation plan

## Prerequisites

- .NET 10 SDK
- Node.js 22 or later
- npm

## Build the backend

```bash
dotnet build SwimmingCoach.sln
```

## Run the backend tests

```bash
dotnet test SwimmingCoach.sln
```

## Run the backend

```bash
dotnet run --project src/SwimmingCoach.Api
```

The health endpoint is available at:

```text
http://localhost:5038/api/health
```

## Install frontend dependencies

```bash
npm --prefix src/swimming-coach-web install
```

## Run the frontend

```bash
npm --prefix src/swimming-coach-web run dev
```

The development site is normally available at:

```text
http://localhost:5173
```

## Build the frontend

```bash
npm --prefix src/swimming-coach-web run build
```

## Lint the frontend

```bash
npm --prefix src/swimming-coach-web run lint
```