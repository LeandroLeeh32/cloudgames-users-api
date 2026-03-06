
using Microsoft.Extensions.Logging;
using Users.Application.Interfaces.Repositories;
using Users.Domain.Exceptions;

namespace Users.Application.UseCases.Users
{
    public class DeleteUserUseCase
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<DeleteUserUseCase> _logger;

        public DeleteUserUseCase(IUserRepository repository, ILogger<DeleteUserUseCase> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task ExecuteAsync(Guid id)
        {
            _logger.LogInformation("[App][DeleteUserUseCase] Starting deactivate for user {UserId}", id);

            var user = await _repository.GetByIdAsync(id);

            if (user is null)
                throw new DomainException("User not found.");

            user.Deactivate();

            await _repository.UpdateAsync(user);

            _logger.LogInformation("[App][DeleteUserUseCase] User {UserId} deactivated", id);
        }
    }

}

