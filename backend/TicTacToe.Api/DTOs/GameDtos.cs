using TicTacToe.Api.Models;

namespace TicTacToe.Api.DTOs;

public sealed record CreateGameRequest(GameMode Mode);

public sealed record MoveRequest(Player Player, int Row, int Column);

public sealed record MoveDto(int Number, Player Player, int Row, int Column);

public sealed record GameStateDto(
    Guid Id,
    string?[][] Board,
    Player CurrentPlayer,
    GameMode Mode,
    GameStatus Status,
    Player? Winner,
    IReadOnlyList<Position> WinningCells,
    IReadOnlyList<MoveDto> MoveHistory,
    Scoreboard Scoreboard);

public sealed record ErrorResponse(string Message);
