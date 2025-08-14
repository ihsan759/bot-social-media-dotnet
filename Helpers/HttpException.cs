public class HttpException : Exception
{
    public int StatusCode { get; }
    public object Errors { get; }
    public bool IsLogging { get; }

    public HttpException(string message, int statusCode = 500, object? errors = null, bool isLogging = false)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors ?? new { error = message };
        IsLogging = isLogging;
    }
}
