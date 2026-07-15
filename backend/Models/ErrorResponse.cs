namespace ComplaintManagementSystem.Models;

public class ErrorResponse
{
    public int StatusCode { get; set; }

    public string Message { get; set; } = string.Empty;
    
    public string? CorrelationId { get; set; }
    
    public DateTime Timestamp { get; set; }
}