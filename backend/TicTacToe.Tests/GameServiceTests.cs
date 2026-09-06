using System;
using Xunit;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;
using System.Linq;

namespace TicTacToe.Tests;

public class GameServiceTests
{
    private static GameService CreateService(out GameStore store)
    {
        store = new GameStore();
        var rules = new GameRules();
        return new GameService(store, rules, new ComputerPlayerService(rules));
    }

    private static GameStateDto Move(GameService service, Guid id, Player player, int row, int col) =>
        service.MakeMove(id, new MoveRequest(player, row, col));

    [Fact]
    public void ValidMove_IsApplied_AndTurnSwitches()
    {
        var service = CreateService(out _);
        var game = service.Create(GameMode.TwoPlayer);

        var state = Move(service, game.Id, Player.X, 0, 0);

        Assert.Equal("X", state.Board[0][0]);
        Assert.Equal(Player.O, state.CurrentPlayer);
        Assert.Single(state.MoveHistory);
    }

    [Fact]
    public void OccupiedMove_IsRejected()
    {
        var service = CreateService(out _);
        var game = service.Create(GameMode.TwoPlayer);
        Move(service, game.Id, Player.X, 0, 0);

        Assert.Throws<InvalidOperationException>(() =>
            Move(service, game.Id, Player.O, 0, 0));
    }

    [Fact]
    public void WrongTurn_IsRejected()
    {
        var service = CreateService(out _);
        var game = service.Create(GameMode.TwoPlayer);

        Assert.Throws<InvalidOperationException>(() =>
            Move(service, game.Id, Player.O, 0, 0));
    }

    [Fact]
    public void RowWin_IsDetected_AndScoreUpdated()
    {
        var service = CreateService(out _);
        var game = service.Create(GameMode.TwoPlayer);

        Move(service, game.Id, Player.X, 0, 0);
        Move(service, game.Id, Player.O, 1, 0);
        Move(service, game.Id, Player.X, 0, 1);
        Move(service, game.Id, Player.O, 1, 1);
        var state = Move(service, game.Id, Player.X, 0, 2);

        Assert.Equal(GameStatus.Won, state.Status);
        Assert.Equal(Player.X, state.Winner);
        Assert.Equal(3, state.WinningCells.Count);
        Assert.Equal(1, state.Scoreboard.XWins);
    }

    [Fact]
    public void ColumnWin_IsDetected()
    {
        var service = CreateService(out _);
        var game = service.Create(GameMode.TwoPlayer);

        Move(service, game.Id, Player.X, 0, 0);
        Move(service, game.Id, Player.O, 0, 1);
        Move(service, game.Id, Player.X, 1, 0);
        Move(service, game.Id, Player.O, 1, 1);
        var state = Move(service, game.Id, Player.X, 2, 0);

        Assert.Equal(GameStatus.Won, state.Status);
        Assert.Equal(Player.X, state.Winner);
    }

    [Fact]
    public void DiagonalWin_IsDetected()
    {
        var service = CreateService(out _);
        var game = service.Create(GameMode.TwoPlayer);

        Move(service, game.Id, Player.X, 0, 0);
        Move(service, game.Id, Player.O, 0, 1);
        Move(service, game.Id, Player.X, 1, 1);
        Move(service, game.Id, Player.O, 0, 2);
        var state = Move(service, game.Id, Player.X, 2, 2);

        Assert.Equal(GameStatus.Won, state.Status);
        Assert.Equal(Player.X, state.Winner);
    }

    [Fact]
    public void Draw_IsDetected()
    {
        var service = CreateService(out _);
        var game = service.Create(GameMode.TwoPlayer);

        var moves = new (Player P, int R, int C)[]
        {
            (Player.X,0,0),(Player.O,0,1),(Player.X,0,2),
            (Player.O,1,1),(Player.X,1,0),(Player.O,1,2),
            (Player.X,2,1),(Player.O,2,0),(Player.X,2,2)
        };

        GameStateDto? state = null;
        foreach (var m in moves)
            state = Move(service, game.Id, m.P, m.R, m.C);

        Assert.NotNull(state);
        Assert.Equal(GameStatus.Draw, state!.Status);
        Assert.Equal(1, state.Scoreboard.Draws);
    }

    [Fact]
    public void ResetGame_ClearsBoard_ButKeepsScore()
    {
        var service = CreateService(out _);
        var game = service.Create(GameMode.TwoPlayer);

        Move(service, game.Id, Player.X, 0, 0);
        Move(service, game.Id, Player.O, 1, 0);
        Move(service, game.Id, Player.X, 0, 1);
        Move(service, game.Id, Player.O, 1, 1);
        Move(service, game.Id, Player.X, 0, 2);

        var reset = service.ResetGame(game.Id);

        Assert.All(reset.Board.SelectMany(row => row), cell => Assert.Null(cell));
        Assert.Empty(reset.MoveHistory);
        Assert.Equal(Player.X, reset.CurrentPlayer);
        Assert.Equal(1, reset.Scoreboard.XWins);
    }

    [Fact]
    public void TwoPlayerUndo_RemovesOneMove_AndRestoresTurn()
    {
        var service = CreateService(out _);
        var game = service.Create(GameMode.TwoPlayer);

        Move(service, game.Id, Player.X, 0, 0);
        Move(service, game.Id, Player.O, 1, 1);

        var state = service.Undo(game.Id);

        Assert.Null(state.Board[1][1]);
        Assert.Single(state.MoveHistory);
        Assert.Equal(Player.O, state.CurrentPlayer);
    }

    [Fact]
    public void ComputerMode_UndoRemovesHumanAndComputerMoves()
    {
        var service = CreateService(out _);
        var game = service.Create(GameMode.Computer);

        var afterHuman = Move(service, game.Id, Player.X, 0, 0);
        Assert.Equal(2, afterHuman.MoveHistory.Count);
        Assert.Equal(Player.X, afterHuman.CurrentPlayer);

        var state = service.Undo(game.Id);

        Assert.Empty(state.MoveHistory);
        Assert.Equal(Player.X, state.CurrentPlayer);
        Assert.All(state.Board.SelectMany(row => row), cell => Assert.Null(cell));
    }

    [Fact]
    public void CompletedGame_RejectsFurtherMove()
    {
        var service = CreateService(out _);
        var game = service.Create(GameMode.TwoPlayer);

        Move(service, game.Id, Player.X, 0, 0);
        Move(service, game.Id, Player.O, 1, 0);
        Move(service, game.Id, Player.X, 0, 1);
        Move(service, game.Id, Player.O, 1, 1);
        Move(service, game.Id, Player.X, 0, 2);

        Assert.Throws<InvalidOperationException>(() =>
            Move(service, game.Id, Player.O, 2, 2));
    }

    [Fact]
    public void ComputerChoosesWinningMove()
    {
        var board = new Player?[3,3];
        board[0,0] = Player.O;
        board[0,1] = Player.O;
        board[1,0] = Player.X;

        var position = new ComputerPlayerService(new GameRules()).ChooseMove(board);

        Assert.Equal(new Position(0,2), position);
    }

    [Fact]
    public void ComputerBlocksWinningMove()
    {
        var board = new Player?[3,3];
        board[0,0] = Player.X;
        board[0,1] = Player.X;
        board[1,1] = Player.O;

        var position = new ComputerPlayerService(new GameRules()).ChooseMove(board);

        Assert.Equal(new Position(0,2), position);
    }
}
