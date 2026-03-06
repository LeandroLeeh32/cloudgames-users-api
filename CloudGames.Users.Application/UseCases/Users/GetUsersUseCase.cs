using Microsoft.Extensions.Logging;
using Users.Application.Interfaces.Repositories;
using Users.Domain.Entities;

namespace Users.Application.UseCases.Users
{
    public class GetUsersUseCase
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<GetUsersUseCase> _logger;

        public GetUsersUseCase(
            IUserRepository repository,
            ILogger<GetUsersUseCase> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> ExecuteAsync()
        {
            _logger.LogInformation("[App][GetUsersUseCase] Fetching all users");

            var users = await _repository.GetAllAsync();

            _logger.LogInformation("[App][GetUsersUseCase] Users found: {Total}",
                users.Count());

            return users;
        }
    }
}
