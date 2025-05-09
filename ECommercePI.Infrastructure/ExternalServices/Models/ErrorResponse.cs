using System.Text.Json.Serialization;

namespace ECommercePI.Infrastructure.ExternalServices.Models;

public class ErrorResponse
{
    [JsonPropertyName("error")] public string? Error { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
}