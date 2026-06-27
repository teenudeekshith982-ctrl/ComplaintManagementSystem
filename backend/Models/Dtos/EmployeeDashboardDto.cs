namespace ComplaintManagementSystem.Models.Dtos;

public class EmployeeDashboardDto
{
    public int AssignedComplaints { get; set; }

    public int InProgressComplaints { get; set; }

    public int ResolvedComplaints { get; set; }

    public int EscalatedComplaints { get; set; }

    public int OverdueComplaints { get; set; }
}