
using Microsoft.Extensions.Logging;
using Users.Application.Interfaces.Repositories;
using Users.Domain.Enums;
using Users.Domain.Exceptions;

namespace Users.Application.UseCases.Users
{
    public class UpdateUserUseCase
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<UpdateUserUseCase> _logger;

        public UpdateUserUseCase(
            IUserRepository repository,
            ILogger<UpdateUserUseCase> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task ExecuteAsync(
            Guid id,
            string name,
            UserRole role)
        {
            _logger.LogInformation(
                "[App][UpdateUserUseCase] Starting update for user {UserId}",
                id);

            var user = await _repository.GetByIdAsync(id);

            if (user is null)
                throw new DomainException("User not found.");

            user.Update(name, role);

            await _repository.UpdateAsync(user);

            _logger.LogInformation(
                "[App][UpdateUserUseCase] User {UserId} updated successfully",
                id);
        }
    }

}

