# Tic Tac Toe — Angular + .NET Web API

A browser-based Tic Tac Toe application with an Angular frontend and ASP.NET Core Web API backend.

## Project overview

The backend is the source of truth for game state, move validation, move history, game status, undo behavior, and the session scoreboard. The Angular application communicates with the backend through REST APIs and renders the returned state.

## Tech stack

- Frontend: Angular + TypeScript
- Backend: ASP.NET Core Web API (.NET 8)
- API style: REST
- Storage: In-memory
- Tests: xUnit
- Source control: Git/GitHub

## Features

- 3x3 Tic Tac Toe
- Two Player mode
- Play Against Computer mode
- Server-side move validation
- Row, column and diagonal win detection
- Draw detection
- Winning-cell highlighting
- Move history
- Undo
- Session scoreboard
- Reset game
- Reset scoreboard
- Deterministic computer strategy:
  1. Win if possible
  2. Block X if needed
  3. Take center
  4. Take a corner
  5. Take any available cell

## Design decisions

### Backend owns state

The Angular client never decides whether a move is valid or whether a game has been won. It submits a move and renders the authoritative state returned by the API.

### In-memory storage

This is intentional for a coding exercise. A `GameStore` singleton keeps games and the session scoreboard in memory. Restarting the backend clears the session.

### Undo after completion

Option A was selected: **Undo is disabled after a game is completed**. This keeps the completed scoreboard result final and avoids having to reverse a previously recorded result.

### Computer mode

X is always the human and O is always the computer. A human move is submitted through the same move endpoint. The backend then makes the computer's move automatically when appropriate.

## API

| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/api/games` | Create a game |
| GET | `/api/games/{id}` | Get game state |
| POST | `/api/games/{id}/moves` | Submit a move |
| POST | `/api/games/{id}/undo` | Undo |
| POST | `/api/games/{id}/reset` | Reset current game |
| GET | `/api/scoreboard` | Get scoreboard |
| POST | `/api/scoreboard/reset` | Reset scoreboard |

### Create game

`POST /api/games`

```json
{
  "mode": "TwoPlayer"
}
```

or:

```json
{
  "mode": "Computer"
}
```

### Move

`POST /api/games/{id}/moves`

```json
{
  "player": "X",
  "row": 0,
  "column": 0
}
```

Rows and columns are zero-based.

### Game state

The API returns:

- game ID
- board
- current player
- mode
- status
- winner
- winning cells
- move history
- scoreboard

## Run backend

Prerequisite: .NET 8 SDK.

```bash
cd backend/TicTacToe.Api
dotnet restore
dotnet run
```

The API listens on:

`http://localhost:5000`

Swagger is available at:

`http://localhost:5000/swagger`

## Run backend tests

```bash
cd backend
dotnet test
```

## Run frontend

Prerequisite: Node.js and Angular CLI.

```bash
cd frontend/tic-tac-toe
npm install
npm start
```

Open:

`http://localhost:4200`

The Angular development server proxies `/api` calls to the backend at `http://localhost:5000`.

## AI-assisted development

AI assistance was used to accelerate scaffolding, implementation ideas, test-case generation, and README drafting. The important implementation choices were reviewed manually, especially:

- backend state ownership
- move validation
- win/draw transitions
- scoreboard update exactly once
- undo semantics
- computer move priority
- API/frontend contract

The final implementation and design decisions were reviewed and understood by the developer, and the application was tested locally before submission.

## Known limitations

- State is lost when the backend restarts.
- There is no authentication or multi-user persistence.
- A single in-memory scoreboard is shared by all games in the running backend.
- The computer strategy is intentionally basic rather than minimax.

## Future improvements

- SQLite persistence
- authentication and player profiles
- persistent match history
- stronger AI using minimax
- integration/e2e tests
- production deployment configuration
