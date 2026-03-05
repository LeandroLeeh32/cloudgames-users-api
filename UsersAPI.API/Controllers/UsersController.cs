
using Microsoft.AspNetCore.Mvc;
using Users.API.Attributes;
using Users.API.Contracts;
using Users.Application.UseCases.Users;

namespace Users.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "Admin")]
    [ApiDefaultResponses]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly CreateUserUseCase _create;
        private readonly GetUsersUseCase _get;
        private readonly UpdateUserUseCase _update;
        private readonly DeleteUserUseCase _delete;
        private readonly GetUserByIdUseCase _getById;

        public UsersController( CreateUserUseCase create, GetUsersUseCase get,UpdateUserUseCase update,DeleteUserUseCase delete, GetUserByIdUseCase getById)
        {
            _create = create;
            _get = get;
            _update = update;
            _delete = delete;
            _getById = getById;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            var id = await _create.ExecuteAsync(
                request.Name,
                request.Email,
                request.Password,
                request.Role);

            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _delete.ExecuteAsync(id);
            return NoContent();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll()
        {
            var users = await _get.ExecuteAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _getById.ExecuteAsync(id);

            if (user is null)
                return NotFound();

            return Ok(user);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
        {
            await _update.ExecuteAsync(id, request.Name, request.Email, request.Role);
            return NoContent();
        }
    }
}

