namespace ActiLink.Services
{
    public class GenericServiceResult<T>
    {
        private ServiceResult InternalServiceResult { get; init; } = ServiceResult.Success();

        public T? Data { get; private init; }
        public bool Succeeded => InternalServiceResult.Succeeded;
        public IEnumerable<string> Errors => InternalServiceResult.Errors;
        public ErrorCode ErrorCode => InternalServiceResult.ErrorCode;

        private GenericServiceResult() { }
        public static GenericServiceResult<T> Success(T data) => new() { Data = data };
        public static GenericServiceResult<T> Failure(IEnumerable<string> errors) => new() { InternalServiceResult = ServiceResult.Failure(errors) };
        public static GenericServiceResult<T> Failure(IEnumerable<string> errors, ErrorCode errorCode) => new() { InternalServiceResult = ServiceResult.Failure(errors, errorCode) };
    }
}
