
using CloudGames.Users.API.Contracts.Request.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Users.Application.UseCases.Auth;

namespace Users.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly LoginUseCase _login;

        public AuthController(LoginUseCase login)
        {
            _login = login;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            var token = await _login.ExecuteAsync(request.Email, request.Password);

            return Ok(new { token });
        }
    }
}
