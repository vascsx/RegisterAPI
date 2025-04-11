namespace RegisterAPI.Interfaces
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new List<string>(); // `set` agora é público

        public static ServiceResult SuccessResult() => new ServiceResult { Success = true };
        public static ServiceResult FailureResult(params string[] errors) =>
            new ServiceResult { Success = false, Errors = errors.ToList() };
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T Data { get; set; }

        public static ServiceResult<T> SuccessResult(T data) =>
            new ServiceResult<T> { Success = true, Data = data };
        public static new ServiceResult<T> FailureResult(params string[] errors) =>
            new ServiceResult<T> { Success = false, Errors = errors.ToList() };
    }
}