namespace ActiLink.Shared.ServiceUtils
{
    public class ServiceResult
    {
        public bool Succeeded { get; private init; }
        public IEnumerable<string> Errors { get; private init; } = [];
        public ErrorCode ErrorCode { get; private init; } = ErrorCode.None;

        private ServiceResult() { }

        public static ServiceResult Success() => new() { Succeeded = true };
        public static ServiceResult Failure(IEnumerable<string> errors) => new() { Succeeded = false, Errors = errors, ErrorCode = ErrorCode.GeneralError };
        public static ServiceResult Failure(IEnumerable<string> errors, ErrorCode errorCode) => new() { Succeeded = false, Errors = errors, ErrorCode = errorCode };
    }
}
