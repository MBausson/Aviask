namespace AviaskApi.Models.Result;

public class ApiErrorResponse(string message)
{
    public string Message { get; private set; } = message;
}