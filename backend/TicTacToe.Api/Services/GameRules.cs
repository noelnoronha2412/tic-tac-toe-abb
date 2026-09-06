using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public class GameRules
{
    public bool IsValidPosition(int row, int column) =>
        row >= 0 && row < 3 && column >= 0 && column < 3;

    public Player? FindWinner(Player?[,] board)
    {
        foreach (var line in GetLines())
        {
            var first = board[line[0].Row, line[0].Column];
            if (first is not null &&
                first == board[line[1].Row, line[1].Column] &&
                first == board[line[2].Row, line[2].Column])
                return first;
        }
        return null;
    }

    public List<Position> FindWinningCells(Player?[,] board, Player player)
    {
        foreach (var line in GetLines())
            if (line.All(p => board[p.Row, p.Column] == player))
                return line;
        return [];
    }

    public bool IsDraw(Player?[,] board) =>
        FindWinner(board) is null && board.Cast<Player?>().All(cell => cell is not null);

    public IEnumerable<Position> GetAvailableCells(Player?[,] board)
    {
        for (var row = 0; row < 3; row++)
        for (var column = 0; column < 3; column++)
            if (board[row, column] is null)
                yield return new Position(row, column);
    }

    private static IEnumerable<List<Position>> GetLines()
    {
        for (var i = 0; i < 3; i++)
        {
            yield return [new Position(i, 0), new Position(i, 1), new Position(i, 2)];
            yield return [new Position(0, i), new Position(1, i), new Position(2, i)];
        }
        yield return [new Position(0, 0), new Position(1, 1), new Position(2, 2)];
        yield return [new Position(0, 2), new Position(1, 1), new Position(2, 0)];
    }
}
