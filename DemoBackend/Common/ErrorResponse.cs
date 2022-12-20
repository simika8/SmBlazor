namespace Common;

public class ErrorResponse
{
    public int Code { get; set;}
    public string Message { get; set; } = null!;
    public object Details { get; set; }

    public ErrorResponse(int code, string message, object? details = null)
    {
        this.Code = code;
        this.Message = message;
        if (details != null)
            this.Details = details;
    }
}
