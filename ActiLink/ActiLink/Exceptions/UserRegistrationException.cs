using Microsoft.AspNetCore.Identity;

namespace ActiLink.Exceptions
{
    public class UserRegistrationException : Exception
    {
        public UserRegistrationException(string message, IEnumerable<IdentityError> errors) : base(message)
        {
            Errors = errors;
        }
        public IEnumerable<IdentityError> Errors { get; }
    }
}
