namespace ActiLink.Services
{
    public class ServiceResult
    {
        public bool Succeeded { get; private init; }
        public IEnumerable<string> Errors { get; private init; } = [];

        private ServiceResult() { }

        public static ServiceResult Success() => new() { Succeeded = true };
        public static ServiceResult Failure(IEnumerable<string> errors) => new() { Succeeded = false, Errors = errors };
    }
}
