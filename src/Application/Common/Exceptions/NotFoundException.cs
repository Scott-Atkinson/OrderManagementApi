namespace Order_Management.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException() : base() { }

    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string name, object key) : base($"{name} - (With ID {key}) was not found.") { }

    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
}
