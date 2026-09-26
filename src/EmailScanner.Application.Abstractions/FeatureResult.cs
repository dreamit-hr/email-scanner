namespace EmailScanner.Application.Abstractions;

public sealed record FeatureError(string Code, string Message);

public sealed record FeatureResult<T>(bool IsSuccess, T? Value, int StatusCode, IReadOnlyList<FeatureError> Errors)
{
    public static FeatureResult<T> Success(T value, int statusCode = 200) => new(true, value, statusCode, []);
    public static FeatureResult<T> Failure(string code, string message, int statusCode = 500) => new(false, default, statusCode, [new(code, message)]);
    public static FeatureResult<T> Validation(string message) => Failure("validation_error", message, 400);
    public static FeatureResult<T> Unauthorized() => Failure("unauthorized", "Authentication is required.", 401);
    public static FeatureResult<T> Forbidden() => Failure("forbidden", "The client does not have permission.", 403);
    public static FeatureResult<T> NotFound(string message = "The requested resource was not found.") => Failure("not_found", message, 404);
}
