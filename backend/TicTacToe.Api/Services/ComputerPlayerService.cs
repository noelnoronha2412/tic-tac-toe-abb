using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public class ComputerPlayerService
{
    private readonly GameRules _rules;

    public ComputerPlayerService(GameRules rules) => _rules = rules;

    public Position ChooseMove(Player?[,] board)
    {
        var winningMove = FindImmediateMove(board, Player.O);
        if (winningMove is not null) return winningMove;

        var blockingMove = FindImmediateMove(board, Player.X);
        if (blockingMove is not null) return blockingMove;

        if (board[1, 1] is null) return new Position(1, 1);

        foreach (var corner in new[]
        {
            new Position(0, 0), new Position(0, 2),
            new Position(2, 0), new Position(2, 2)
        })
            if (board[corner.Row, corner.Column] is null) return corner;

        return _rules.GetAvailableCells(board).First();
    }

    private Position? FindImmediateMove(Player?[,] board, Player player)
    {
        foreach (var position in _rules.GetAvailableCells(board))
        {
            board[position.Row, position.Column] = player;
            var wins = _rules.FindWinner(board) == player;
            board[position.Row, position.Column] = null;
            if (wins) return position;
        }
        return null;
    }
}
