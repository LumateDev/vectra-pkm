using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vectra.Modules.Identity.Application.DTOs.Requests;
using Vectra.Modules.Identity.Application.DTOs.Responses;
using Vectra.Modules.Identity.Application.Exceptions;
using Vectra.Modules.Identity.Application.Services;
using Vectra.API.Dtos;

namespace Vectra.API.Modules.Identity.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="request">Registration details</param>
        /// <returns>Created user information</returns>
        /// <response code="201">User successfully created</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="409">User already exists</response>
        [HttpPost("register", Name = "Register")]
        [AllowAnonymous]
        [ApiExplorerSettings(GroupName = "v1")]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                return CreatedAtAction(nameof(GetCurrentUser), new { }, result);
            }
            catch (UserAlreadyExistsException ex)
            {
                _logger.LogWarning("Registration failed: {Message}", ex.Message);
                return Conflict(new ApiErrorResponse
                {
                    Title = "Registration Failed",
                    Detail = ex.Message,
                    Status = StatusCodes.Status409Conflict,
                    Code = ex.Code 
                });
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Title = "Validation Failed",
                    Detail = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest,
                    Errors = ex.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        )
                });
            }
        }

        /// <summary>
        /// Login with email/username and password
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Access and refresh tokens</returns>
        /// <response code="200">Successfully authenticated</response>
        /// <response code="401">Invalid credentials</response>
        /// <response code="403">User account is not active</response>
        [HttpPost("login", Name = "Login")]
        [AllowAnonymous]
        [ApiExplorerSettings(GroupName = "v1")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var ipAddress = GetIpAddress();
                var result = await _authService.LoginAsync(request, ipAddress);

                SetRefreshTokenCookie(result.RefreshToken);

                return Ok(result);
            }
            catch (InvalidCredentialsException ex)
            {
                return Unauthorized(new ApiErrorResponse
                {
                    Title = "Authentication Failed",
                    Detail = ex.Message,
                    Status = StatusCodes.Status401Unauthorized,
                    Code = ex.Code
                });
            }
            catch (UserNotActiveException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse
                {
                    Title = "Account Inactive",
                    Detail = ex.Message,
                    Status = StatusCodes.Status403Forbidden,
                    Code = ex.Code
                });
            }
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="request">Refresh token</param>
        /// <returns>New access and refresh tokens</returns>
        /// <response code="200">Tokens successfully refreshed</response>
        /// <response code="401">Invalid or expired refresh token</response>
        [HttpPost("refresh", Name = "RefreshToken")]
        [AllowAnonymous]
        [ApiExplorerSettings(GroupName = "v1")]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest? request)
        {
            try
            {
                var refreshToken = request?.RefreshToken ?? Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(new ApiErrorResponse
                    {
                        Title = "Token Required",
                        Detail = "Refresh token is required",
                        Status = StatusCodes.Status401Unauthorized
                    });
                }

                var ipAddress = GetIpAddress();
                var result = await _authService.RefreshTokenAsync(
                    new RefreshTokenRequest { RefreshToken = refreshToken }, ipAddress);

                SetRefreshTokenCookie(result.RefreshToken);

                return Ok(result);
            }
            catch (InvalidTokenException ex)
            {
                return Unauthorized(new ApiErrorResponse
                {
                    Title = "Token Invalid",
                    Detail = ex.Message,
                    Status = StatusCodes.Status401Unauthorized,
                    Code = ex.Code
                });
            }
        }

        /// <summary>
        /// Logout and revoke refresh token
        /// </summary>
        /// <returns>No content</returns>
        /// <response code="204">Successfully logged out</response>
        [HttpPost("logout", Name = "Logout")]
        [Authorize]
        [ApiExplorerSettings(GroupName = "v1")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var ipAddress = GetIpAddress();
                await _authService.RevokeTokenAsync(refreshToken, ipAddress);
            }

            Response.Cookies.Delete("refreshToken");

            return NoContent();
        }

        /// <summary>
        /// Get current user information
        /// </summary>
        /// <returns>Current user details</returns>
        /// <response code="200">User information retrieved</response>
        /// <response code="401">Not authenticated</response>
        [HttpGet("me", Name = "GetCurrentUser")]
        [Authorize]
        [ApiExplorerSettings(GroupName = "v1")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiErrorResponse
                {
                    Title = "Unauthorized",
                    Detail = "User is not authenticated.",
                    Status = StatusCodes.Status401Unauthorized
                });
            }

            var user = await _authService.GetUserByIdAsync(Guid.Parse(userId));
            if (user == null)
            {
                return NotFound(new ApiErrorResponse
                {
                    Title = "Not Found",
                    Detail = "User not found.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(user);
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Требует HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        private string? GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return Request.Headers["X-Forwarded-For"];
            }

            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        }
    }
}