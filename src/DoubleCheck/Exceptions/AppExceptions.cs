namespace DoubleCheck.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string message) : base(message) { }
}

public sealed class NotFoundException : AppException { public NotFoundException(string m) : base(m) { } }   // 404
public sealed class ForbiddenException : AppException { public ForbiddenException(string m) : base(m) { } }  // 403
public sealed class ValidationException : AppException { public ValidationException(string m) : base(m) { } }// 400
public sealed class ConflictException : AppException { public ConflictException(string m) : base(m) { } }    // 409
public sealed class DomainException : AppException { public DomainException(string m) : base(m) { } }         // 400
