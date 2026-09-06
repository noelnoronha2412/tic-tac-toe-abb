using System;
using Xunit;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Tests;

public class GameRulesTests
{
    private readonly GameRules _rules = new();

    [Fact]
    public void FindWinner_DetectsColumn()
    {
        var board = new Player?[3, 3];
        board[0, 1] = Player.O;
        board[1, 1] = Player.O;
        board[2, 1] = Player.O;

        Assert.Equal(Player.O, _rules.FindWinner(board));
    }

    [Fact]
    public void IsDraw_ReturnsTrueForFullBoardWithoutWinner()
    {
        var board = new Player?[3, 3];
        var values = new Player?[]
        { Player.X, Player.O, Player.X, Player.X, Player.O, Player.O, Player.O, Player.X, Player.X };

        for (var i = 0; i < 9; i++)
            board[i / 3, i % 3] = values[i];

        Assert.True(_rules.IsDraw(board));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 3)]
    [InlineData(3, 0)]
    public void IsValidPosition_RejectsOutsideBoard(int row, int column)
    {
        Assert.False(_rules.IsValidPosition(row, column));
    }
}
