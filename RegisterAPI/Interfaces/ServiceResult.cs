namespace RegisterAPI.Interfaces
{
    public class ServiceResult
    {
        public bool Success { get; set; } // Indicates if the operation was successful
        public List<string> Errors { get; set; } = new List<string>(); // Stores error messages

        // Factory method for success result
        public static ServiceResult SuccessResult() => new ServiceResult { Success = true };

        // Factory methods for failure result
        public static ServiceResult FailureResult(params string[] errors) =>
            new ServiceResult { Success = false, Errors = errors.ToList() };

        public static ServiceResult FailureResult(IEnumerable<string> errors) =>
            new ServiceResult { Success = false, Errors = errors.ToList() };
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T Data { get; set; } // Holds the data result for generic responses

        // Factory method for success result with data
        public static ServiceResult<T> SuccessResult(T data) =>
            new ServiceResult<T> { Success = true, Data = data };

        // Factory methods for failure result
        public static new ServiceResult<T> FailureResult(params string[] errors) =>
            new ServiceResult<T> { Success = false, Errors = errors.ToList() };

        public static new ServiceResult<T> FailureResult(IEnumerable<string> errors) =>
            new ServiceResult<T> { Success = false, Errors = errors.ToList() };
    }
}