namespace ECommercePI.Domain.Common;

public class BalanceServiceResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public BalanceServiceResponse()
    {
    }

    private BalanceServiceResponse(bool success, string? message = null, T? data = default)
    {
        Success = success;
        Message = message;
        Data = data;
    }

    public static BalanceServiceResponse<T> Ok(T data, string? message = null)
        => new(true, message, data);

    public static BalanceServiceResponse<T> Fail(string message)
        => new(false, message);
}