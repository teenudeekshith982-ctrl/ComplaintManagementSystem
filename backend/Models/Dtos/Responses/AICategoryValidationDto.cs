namespace ComplaintManagementSystem.Models.Dtos.Responses
{
    public class AICategoryValidationDto
    {
        public string SuggestedCategory { get; set; } = string.Empty;
        public string Confidence { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
