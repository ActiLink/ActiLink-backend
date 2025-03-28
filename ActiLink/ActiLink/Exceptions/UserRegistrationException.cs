using Microsoft.AspNetCore.Identity;

namespace ActiLink.Exceptions
{
    /// <summary>
    /// Exception thrown when user registration fails
    /// </summary>
    public class UserRegistrationException : Exception
    {
        public UserRegistrationException(string message, IEnumerable<IdentityError> errors) : base(message)
        {
            Errors = errors;
        }
        public IEnumerable<IdentityError> Errors { get; }
    }
}
