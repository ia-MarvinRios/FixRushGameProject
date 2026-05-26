using FixRushGameAPI.DTOs;
using FixRushGameAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FixRushGameAPI.Controller
{
    [ApiController]
    [Route("api/players")]
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerService _playerService;

        public PlayerController(IPlayerService playerService) => _playerService = playerService;

        /// PUT /api/players/{playerId}/dinero
        /// Body: { "dinero": 1500.00 }
        [HttpPut("{playerId}/dinero")]
        public async Task<IActionResult> ActualizarDinero(int playerId, [FromBody] UpdateDineroRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Valor de dinero invalido."));

            var (success, message, dinero) = await _playerService.ActualizarDinero(playerId, request.Dinero);

            if (!success)
                return NotFound(ApiResponse<object>.Fail(message));

            return Ok(ApiResponse<object>.Ok(new { dinero }, message));
        }

        /// PUT /api/players/{playerId}/items/{itemId}
        /// Body: { "tiene": true }
        [HttpPut("{playerId}/items/{itemId}")]
        public async Task<IActionResult> ActualizarItem(int playerId, int itemId, [FromBody] UpdateItemRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Request invalido."));

            var (success, message) = await _playerService.ActualizarItem(playerId, itemId, request.Tiene);

            if (!success)
                return NotFound(ApiResponse<object>.Fail(message));

            return Ok(ApiResponse<object>.Ok(new { playerId, itemId, tiene = request.Tiene }, message));
        }
    }
}
