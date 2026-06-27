namespace ComplaintManagementSystem.Models.Dtos;

public class UserDashboardDto
{
    public int TotalComplaints { get; set; }

    public int OpenComplaints { get; set; }

    public int AssignedComplaints { get; set; }

    public int InProgressComplaints { get; set; }

    public int ResolvedComplaints { get; set; }

    public int ClosedComplaints { get; set; }
}