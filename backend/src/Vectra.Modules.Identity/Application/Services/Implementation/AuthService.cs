using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Vectra.Modules.Identity.Application.DTOs.Requests;
using Vectra.Modules.Identity.Application.DTOs.Responses;
using Vectra.Modules.Identity.Application.Exceptions;
using Vectra.Modules.Identity.Domain.Entities;
using Vectra.Modules.Identity.Domain.Enums;
using Vectra.Modules.Identity.Domain.Repositories;

namespace Vectra.Modules.Identity.Application.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly IValidator<LoginRequest> _loginValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Email regex pattern
        private static readonly Regex EmailRegex = new(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IUnitOfWork unitOfWork,
            IValidator<RegisterRequest> registerValidator,
            IValidator<LoginRequest> loginValidator,
            IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            // Валидация
            var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // Проверяем существование пользователя
            var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser != null)
            {
                throw new UserAlreadyExistsException("email", request.Email);
            }

            existingUser = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
            if (existingUser != null)
            {
                throw new UserAlreadyExistsException("username", request.Username);
            }

            // Создаем пользователя
            var passwordHash = _passwordHasher.HashPassword(request.Password);
            var user = new User(request.Email, request.Username, passwordHash);

            // Сохраняем через Unit of Work
            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new RegisterResponse
            {
                UserId = user.Id,
                Email = user.Email,
                Username = user.Username,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
        {
            // Валидация
            var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // Определяем, это email или username
            User? user;
            if (IsValidEmail(request.EmailOrUsername))
            {
                user = await _userRepository.GetByEmailAsync(request.EmailOrUsername, cancellationToken);
            }
            else
            {
                user = await _userRepository.GetByUsernameAsync(request.EmailOrUsername, cancellationToken);
            }

            if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new InvalidCredentialsException();
            }

            // Проверяем статус пользователя
            if (user.Status != UserStatus.Active)
            {
                throw new UserNotActiveException(user.Status);
            }

            // Проверяем, нужно ли обновить хеш пароля
            if (_passwordHasher.NeedsRehash(user.PasswordHash))
            {
                var newHash = _passwordHasher.HashPassword(request.Password);
                user.UpdatePassword(newHash);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Генерируем токены
            var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, ipAddress);

            // Сохраняем refresh token через Unit of Work
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = refreshToken.ExpiresAt,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    Role = user.Role.ToString()
                }
            };
        }

        public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
        {
            var (accessToken, refreshToken) = await _tokenService.RefreshTokenAsync(request.RefreshToken, ipAddress);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = refreshToken.ExpiresAt
            };
        }

        public async Task RevokeTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default)
        {
            await _tokenService.RevokeTokenAsync(refreshToken, ipAddress);

            // Если есть текущий access token - добавляем в blacklist
            var currentUser = _httpContextAccessor.HttpContext?.User;
            if (currentUser?.Identity?.IsAuthenticated == true)
            {
                var jti = currentUser.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                var exp = currentUser.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
                var userId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(jti) && !string.IsNullOrEmpty(exp) && Guid.TryParse(userId, out var userGuid))
                {
                    var expiresAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).UtcDateTime;
                    await _tokenService.BlacklistTokenAsync(jti, expiresAt, userGuid, "User logout");
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static bool IsValidEmail(string input)
        {
            return EmailRegex.IsMatch(input);
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return null;
            }

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Role = user.Role.ToString()
            };
        }
    }
}
