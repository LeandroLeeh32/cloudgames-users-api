using Users.Application.Interfaces.Repositories;
using Users.Domain.Exceptions;

namespace Users.Application.UseCases.Auth
{
    public class LoginUseCase
    {
        private readonly IUserRepository _repository;
        private readonly IPasswordHashService _hashService;
        private readonly IJwtTokenGenerator _jwtGenerator;

        public LoginUseCase(
            IUserRepository repository,
            IPasswordHashService hashService,
            IJwtTokenGenerator jwtGenerator)
        {
            _repository = repository;
            _hashService = hashService;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<string> ExecuteAsync(string email, string password)
        {
            var user = await _repository.GetByEmailAsync(email);

            if (user is null || !_hashService.Verify(password, user.PasswordHash))
                throw new DomainException("Invalid credentials.");

            if (!user.IsActive)
                throw new DomainException("User inactive.");

            return _jwtGenerator.Generate(user);
        }
    }
}
