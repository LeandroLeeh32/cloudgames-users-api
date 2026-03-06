using CloudGames.Users.Application.Interfaces.Security;
using Microsoft.Extensions.Logging;
using Users.Application.Interfaces.Repositories;
using Users.Domain.Exceptions;

namespace Users.Application.UseCases.Auth
{
    public class LoginUseCase
    {
        private readonly IUserRepository _repository;
        private readonly IPasswordHashService _hashService;
        private readonly ILogger<LoginUseCase> _logger;
        private readonly IJwtTokenGenerator _jwtGenerator;

        public LoginUseCase(
            IUserRepository repository,
            IPasswordHashService hashService,
            IJwtTokenGenerator jwtGenerator,
            ILogger<LoginUseCase> logger)
        {
            _repository = repository;
            _hashService = hashService;
            _jwtGenerator = jwtGenerator;
            _logger = logger;
        }

        public async Task<string> ExecuteAsync(string email, string password)
        {

            _logger.LogInformation("[APPLICATION][Auth] Initiating login... Email: {Email}", email);
            var user = await _repository.GetByEmailAsync(email);

            if (user is null || !_hashService.Verify(password, user.PasswordHash))
                throw new DomainException("Invalid credentials.");

            if (!user.IsActive)
                throw new DomainException("User inactive.");

            _logger.LogInformation("[APPLICATION][Auth] Login completed successfully. Email: {Email}", email);

            return _jwtGenerator.Generate(user);
        }
    }
}
