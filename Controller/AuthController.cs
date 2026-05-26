using FixRushGameAPI.DTOs;
using FixRushGameAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FixRushGameAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth) => _auth = auth;

        /// POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(ObtenerErrores()));

            var (success, message, data) = await _auth.Registrar(request);

            if (!success)
                return BadRequest(ApiResponse<RegisterResponse>.Fail(message));

            return StatusCode(201, ApiResponse<RegisterResponse>.Ok(data!, message));
        }

        /// POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(ObtenerErrores()));

            var (success, message, data) = await _auth.Login(request);

            if (!success)
                return Unauthorized(ApiResponse<LoginResponse>.Fail(message));

            return Ok(ApiResponse<LoginResponse>.Ok(data!));
        }

        private string ObtenerErrores() =>
            string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
    }
}
