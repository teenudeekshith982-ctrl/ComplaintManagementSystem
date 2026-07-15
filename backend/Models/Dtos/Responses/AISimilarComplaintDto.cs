namespace ComplaintManagementSystem.Models.Dtos.Responses
{
    public class AISimilarComplaintDto
    {
        public int ComplaintId { get; set; }
        public string Similarity { get; set; } = string.Empty;
        public string ComplaintTitle { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ResolutionSummary { get; set; } = string.Empty;
    }
}
