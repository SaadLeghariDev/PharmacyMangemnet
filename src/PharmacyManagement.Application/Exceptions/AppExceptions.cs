namespace PharmacyManagement.Application.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, 404) { }
}

public sealed class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409) { }
}

public sealed class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message = "Unauthorized") : base(message, 401) { }
}

public sealed class ForbiddenAppException : AppException
{
    public ForbiddenAppException(string message = "Forbidden") : base(message, 403) { }
}

public sealed class ValidationAppException : AppException
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationAppException(IEnumerable<string> errors, string message = "Validation failed")
        : base(message, 400)
    {
        Errors = errors.ToList();
    }
}
