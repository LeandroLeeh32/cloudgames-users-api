using CloudGames.Notifications.Application.IntegrationEvents.Users;
using CloudGames.Users.Application.Interfaces.Messaging;
using CloudGames.Users.Application.Interfaces.Security;
using Microsoft.Extensions.Logging;
using Users.Application.Interfaces.Repositories;
using Users.Domain.Entities;
using Users.Domain.Enums;
using Users.Domain.Exceptions;

namespace Users.Application.UseCases.Users
{
    public class CreateUserUseCase
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<CreateUserUseCase> _logger;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IEventPublisher _eventPublisher;

        public CreateUserUseCase(IUserRepository repository, IPasswordHashService passwordHashService, ILogger<CreateUserUseCase> logger, IEventPublisher eventPublisher)
        {
            _repository = repository;
            _passwordHashService = passwordHashService;
            _logger = logger;
            _eventPublisher = eventPublisher;
        }

        public async Task<Guid> ExecuteAsync(string name, string email, string password, UserRole role)
        {
            _logger.LogInformation("[App][CreateUserUseCase] Starting user creation. Email: {Email}", email);

            var existingUser = await _repository.GetByEmailAsync(email);

            if (existingUser is not null)
                throw new DomainException("User already exists.");

            var passwordHash = _passwordHashService.Hash(password);

            var user = User.Create(name, email, passwordHash, role);

            await _repository.AddAsync(user);

            _logger.LogInformation("[App][CreateUserUseCase] User created successfully. Id: {UserId}", user.Id);

            _logger.LogInformation("[App][CreateUserUseCase] Preparing user for publishing. Id: {UserId}", user.Id);
            try
            {
                await _eventPublisher.PublishAsync(new UserCreatedIntegrationEvent(user.Id, user.Name, user.Email));
            }
            catch (Exception)
            {
                _logger.LogError("[App][CreateUserUseCase] Error published. Id: {UserId}", user.Id);
                throw;
            }
           

            _logger.LogInformation("[App][CreateUserUseCase] User successfully published. Id: {UserId}", user.Id);



            return user.Id;
        }
    }
}

