namespace AuthService.Application.Exceptions;

public class BadHttpRequestException:Exception
{
    public BadHttpRequestException(string message) : base(message) { }
}