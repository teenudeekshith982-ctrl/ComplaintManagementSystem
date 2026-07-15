namespace ComplaintManagementSystem.Models.Dtos;

public class DashboardSummaryDto
{
    public int TotalComplaints { get; set; }

    public int OpenComplaints { get; set; }

    public int ResolvedComplaints { get; set; }

    public int ClosedComplaints { get; set; }

    public double AverageResolutionTimeHours { get; set; }

    public double SlaBreachRate { get; set; }

    public int UnassignedTicketsCount { get; set; }

    public int OpenEscalationsCount { get; set; }
}