using TicTacToe.Api.DTOs;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public class GameService
{
    private readonly GameStore _store;
    private readonly GameRules _rules;
    private readonly ComputerPlayerService _computer;

    public GameService(GameStore store, GameRules rules, ComputerPlayerService computer)
    {
        _store = store;
        _rules = rules;
        _computer = computer;
    }

    public GameStateDto Create(GameMode mode)
    {
        lock (_store.SyncRoot)
        {
            var game = new Game { Mode = mode };
            _store.Games[game.Id] = game;
            return ToDto(game);
        }
    }

    public GameStateDto Get(Guid id)
    {
        lock (_store.SyncRoot) return ToDto(GetGame(id));
    }

    public GameStateDto MakeMove(Guid id, MoveRequest request)
    {
        lock (_store.SyncRoot)
        {
            var game = GetGame(id);
            ValidateMove(game, request);
            ApplyMove(game, request.Player, request.Row, request.Column);
            UpdateGameStatus(game);

            if (game.Status == GameStatus.InProgress &&
                game.Mode == GameMode.Computer && game.CurrentPlayer == Player.O)
            {
                var computerMove = _computer.ChooseMove(game.Board);
                ApplyMove(game, Player.O, computerMove.Row, computerMove.Column);
                UpdateGameStatus(game);
            }

            return ToDto(game);
        }
    }

    public GameStateDto Undo(Guid id)
    {
        lock (_store.SyncRoot)
        {
            var game = GetGame(id);
            if (game.Status != GameStatus.InProgress)
                throw new InvalidOperationException("Undo is disabled after the game is completed.");
            if (game.Moves.Count == 0)
                throw new InvalidOperationException("There are no moves to undo.");

            var count = game.Mode == GameMode.Computer ? Math.Min(2, game.Moves.Count) : 1;
            for (var i = 0; i < count; i++)
            {
                var move = game.Moves[^1];
                game.Board[move.Row, move.Column] = null;
                game.Moves.RemoveAt(game.Moves.Count - 1);
            }

            game.CurrentPlayer = game.Mode == GameMode.Computer
                ? Player.X
                : game.Moves.Count % 2 == 0 ? Player.X : Player.O;
            game.Winner = null;
            game.WinningCells.Clear();
            game.Status = GameStatus.InProgress;
            return ToDto(game);
        }
    }

    public GameStateDto ResetGame(Guid id)
    {
        lock (_store.SyncRoot)
        {
            var game = GetGame(id);
            var reset = new Game { Id = game.Id, Mode = game.Mode };
            _store.Games[id] = reset;
            return ToDto(reset);
        }
    }

    public Scoreboard GetScoreboard()
    {
        lock (_store.SyncRoot) return CopyScoreboard();
    }

    public Scoreboard ResetScoreboard()
    {
        lock (_store.SyncRoot)
        {
            _store.Scoreboard.XWins = 0;
            _store.Scoreboard.OWins = 0;
            _store.Scoreboard.Draws = 0;
            return CopyScoreboard();
        }
    }

    private Game GetGame(Guid id) =>
        _store.Games.TryGetValue(id, out var game)
            ? game
            : throw new KeyNotFoundException("Game not found.");

    private void ValidateMove(Game game, MoveRequest request)
    {
        if (game.Status != GameStatus.InProgress)
            throw new InvalidOperationException("The game is already completed.");
        if (!_rules.IsValidPosition(request.Row, request.Column))
            throw new ArgumentException("Row and column must be between 0 and 2.");
        if (request.Player != game.CurrentPlayer)
            throw new InvalidOperationException($"It is {game.CurrentPlayer}'s turn.");
        if (game.Mode == GameMode.Computer && request.Player != Player.X)
            throw new InvalidOperationException("Only X can make human moves in computer mode.");
        if (game.Board[request.Row, request.Column] is not null)
            throw new InvalidOperationException("That cell is already occupied.");
    }

    private static void ApplyMove(Game game, Player player, int row, int column)
    {
        game.Board[row, column] = player;
        game.Moves.Add(new Move(game.Moves.Count + 1, player, row, column));
        game.CurrentPlayer = player == Player.X ? Player.O : Player.X;
    }

    private void UpdateGameStatus(Game game)
    {
        var winner = _rules.FindWinner(game.Board);
        if (winner is not null)
        {
            game.Status = GameStatus.Won;
            game.Winner = winner;
            game.WinningCells = _rules.FindWinningCells(game.Board, winner.Value);
            RecordScore(game);
        }
        else if (_rules.IsDraw(game.Board))
        {
            game.Status = GameStatus.Draw;
            RecordScore(game);
        }
    }

    private void RecordScore(Game game)
    {
        if (game.ScoreRecorded) return;

        if (game.Status == GameStatus.Won && game.Winner == Player.X)
            _store.Scoreboard.XWins++;
        else if (game.Status == GameStatus.Won && game.Winner == Player.O)
            _store.Scoreboard.OWins++;
        else if (game.Status == GameStatus.Draw)
            _store.Scoreboard.Draws++;

        game.ScoreRecorded = true;
    }

    private Scoreboard CopyScoreboard() => new()
    {
        XWins = _store.Scoreboard.XWins,
        OWins = _store.Scoreboard.OWins,
        Draws = _store.Scoreboard.Draws
    };

    private GameStateDto ToDto(Game game)
    {
        var board = new string?[3][];
        for (var row = 0; row < 3; row++)
        {
            board[row] = new string?[3];
            for (var column = 0; column < 3; column++)
                board[row][column] = game.Board[row, column]?.ToString();
        }

        return new GameStateDto(
            game.Id, board, game.CurrentPlayer, game.Mode, game.Status,
            game.Winner, game.WinningCells.ToList(),
            game.Moves.Select(m => new MoveDto(m.Number, m.Player, m.Row, m.Column)).ToList(),
            CopyScoreboard());
    }
}
