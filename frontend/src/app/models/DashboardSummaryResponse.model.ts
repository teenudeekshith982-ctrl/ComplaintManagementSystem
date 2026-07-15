export interface DashboardSummaryResponse {
    totalComplaints: number;
    openComplaints: number;
    resolvedComplaints: number;
    closedComplaints: number;
    averageResolutionTimeHours: number;
    slaBreachRate: number;
    unassignedTicketsCount: number;
    openEscalationsCount: number;
}
