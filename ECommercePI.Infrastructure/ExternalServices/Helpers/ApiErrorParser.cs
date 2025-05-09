using System.Text.Json;
using ECommercePI.Infrastructure.ExternalServices.Models;

namespace ECommercePI.Infrastructure.ExternalServices.Helpers;

public static class ApiErrorParser
{
    public static string? ExtractMessage(string? errorContent)
    {
        if (string.IsNullOrWhiteSpace(errorContent))
            return null;

        try
        {
            var error = JsonSerializer.Deserialize<ErrorResponse>(errorContent);
            return error?.Message ?? error?.Error ?? "Unexpected error format.";
        }
        catch
        {
            return "An error occurred while parsing error content.";
        }
    }
}