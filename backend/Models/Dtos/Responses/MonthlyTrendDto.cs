namespace ComplaintManagementSystem.Models.Dtos
{
    public class MonthlyTrendDto
    {
        public string MonthName { get; set; } = string.Empty;
        public int SubmittedCount { get; set; }
        public int ResolvedCount { get; set; }
    }
}
