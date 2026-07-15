namespace ComplaintManagementSystem.Models.Dtos.Responses
{
    public class AIPriorityRecommendationDto
    {
        public string Priority { get; set; } = string.Empty;
        public string Confidence { get; set; } = string.Empty;
        public string SuggestedSla { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
