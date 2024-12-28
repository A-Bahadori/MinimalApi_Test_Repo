namespace MinimalApi_Test.Result
{
    public class ResultCustom<T>
    {
        public bool IsSuccess { get; }
        public T? Data { get; }
        public string? Error { get; }

        private ResultCustom(bool isSuccess, T? data, string? error)
        {
            IsSuccess = isSuccess;
            Data = data;
            Error = error;
        }

        public static ResultCustom<T> Success(T data) => new(true, data, null);
        public static ResultCustom<T> Failure(string error) => new(false, default, error);
    }
}
