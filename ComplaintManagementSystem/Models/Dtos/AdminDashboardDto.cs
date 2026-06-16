namespace ComplaintManagementSystem.Models.Dtos;

public class AdminDashboardDto
{
    public int TotalComplaints { get; set; }

    public int OpenComplaints { get; set; }

    public int InProgressComplaints { get; set; }

    public int ResolvedComplaints { get; set; }

    public int ClosedComplaints { get; set; }

    public int EscalatedComplaints { get; set; }

    public int TotalEmployees { get; set; }
}