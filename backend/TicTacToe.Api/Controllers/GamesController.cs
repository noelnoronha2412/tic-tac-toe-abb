using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly GameService _service;

    public GamesController(GameService service) => _service = service;

    [HttpPost]
    public ActionResult<GameStateDto> Create(CreateGameRequest request)
        => Ok(_service.Create(request.Mode));

    [HttpGet("{id:guid}")]
    public ActionResult<GameStateDto> Get(Guid id)
    {
        try { return Ok(_service.Get(id)); }
        catch (KeyNotFoundException ex) { return NotFound(new ErrorResponse(ex.Message)); }
    }

    [HttpPost("{id:guid}/moves")]
    public ActionResult<GameStateDto> Move(Guid id, MoveRequest request)
    {
        try { return Ok(_service.MakeMove(id, request)); }
        catch (KeyNotFoundException ex) { return NotFound(new ErrorResponse(ex.Message)); }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    [HttpPost("{id:guid}/undo")]
    public ActionResult<GameStateDto> Undo(Guid id)
    {
        try { return Ok(_service.Undo(id)); }
        catch (KeyNotFoundException ex) { return NotFound(new ErrorResponse(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(new ErrorResponse(ex.Message)); }
    }

    [HttpPost("{id:guid}/reset")]
    public ActionResult<GameStateDto> Reset(Guid id)
    {
        try { return Ok(_service.ResetGame(id)); }
        catch (KeyNotFoundException ex) { return NotFound(new ErrorResponse(ex.Message)); }
    }
}
