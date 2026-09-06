using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/scoreboard")]
public class ScoreboardController : ControllerBase
{
    private readonly GameService _service;

    public ScoreboardController(GameService service) => _service = service;

    [HttpGet]
    public ActionResult<Scoreboard> Get() => Ok(_service.GetScoreboard());

    [HttpPost("reset")]
    public ActionResult<Scoreboard> Reset() => Ok(_service.ResetScoreboard());
}
