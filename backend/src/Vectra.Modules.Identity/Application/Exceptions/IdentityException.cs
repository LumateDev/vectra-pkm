using System.Net;
using Vectra.Modules.Identity.Domain.Enums;

namespace Vectra.Modules.Identity.Application.Exceptions
{
   public abstract class IdentityException : Exception
    {
        public string Code { get; }
        public HttpStatusCode StatusCode { get; protected set; } = HttpStatusCode.BadRequest;

        protected IdentityException(string message, string code) : base(message)
        {
            Code = code;
        }
    }

    public abstract class IdentityOperationException : IdentityException
    {
        protected IdentityOperationException(string message, string code) : base(message, code)
        {
        }
    }

    public class UserAlreadyExistsException : IdentityOperationException
    {
        public string Field { get; }
        public string Value { get; }

        public UserAlreadyExistsException(string field, string value)
            : base($"User with {field} '{value}' already exists", "USER_ALREADY_EXISTS")
        {
            Field = field;
            Value = value;
            StatusCode = HttpStatusCode.Conflict;
        }
    }

    public class InvalidCredentialsException : IdentityOperationException
    {
        public InvalidCredentialsException()
            : base("Invalid email or password", "INVALID_CREDENTIALS")
        {
            StatusCode = HttpStatusCode.Unauthorized;
        }
    }

    public class UserNotActiveException : IdentityOperationException
    {
        public UserStatus Status { get; }

        public UserNotActiveException(UserStatus status)
            : base($"User account is {status.ToString().ToLower()}", "USER_NOT_ACTIVE")
        {
            Status = status;
            StatusCode = HttpStatusCode.Forbidden;
        }
    }

    public class TokenExpiredException : IdentityOperationException
    {
        public TokenExpiredException()
            : base("Token has expired", "TOKEN_EXPIRED")
        {
            StatusCode = HttpStatusCode.Unauthorized;
        }
    }

    public class InvalidTokenException : IdentityOperationException
    {
        public InvalidTokenException()
            : base("Invalid token", "INVALID_TOKEN")
        {
            StatusCode = HttpStatusCode.Unauthorized;
        }
    }
}
