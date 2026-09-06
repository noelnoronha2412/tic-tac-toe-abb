namespace TicTacToe.Api.Models;

public enum Player
{
    X,
    O
}

public enum GameMode
{
    TwoPlayer,
    Computer
}

public enum GameStatus
{
    InProgress,
    Won,
    Draw
}

public sealed record Position(int Row, int Column);

public sealed record Move(
    int Number,
    Player Player,
    int Row,
    int Column);

public class Scoreboard
{
    public int XWins { get; set; }
    public int OWins { get; set; }
    public int Draws { get; set; }
}

public class Game
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Player?[,] Board { get; } = new Player?[3, 3];
    public Player CurrentPlayer { get; set; } = Player.X;
    public GameMode Mode { get; init; }
    public GameStatus Status { get; set; } = GameStatus.InProgress;
    public Player? Winner { get; set; }
    public List<Position> WinningCells { get; set; } = [];
    public List<Move> Moves { get; } = [];
    public bool ScoreRecorded { get; set; }
}
