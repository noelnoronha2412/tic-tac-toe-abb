using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public  class GameStore
{
    public Dictionary<Guid, Game> Games { get; } = [];
    public Scoreboard Scoreboard { get; } = new();

    public readonly object SyncRoot = new();
}
