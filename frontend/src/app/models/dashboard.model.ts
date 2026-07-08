export interface UserDashboardData {
    totalComplaints: number;
    openComplaints: number;
    assignedComplaints: number;
    inProgressComplaints: number;
    resolvedComplaints: number;
    closedComplaints: number;
}

export interface AdminDashboardData {
    totalComplaints: number;
    openComplaints: number;
    inProgressComplaints: number;
    resolvedComplaints: number;
    closedComplaints: number;
    escalatedComplaints: number;
    totalEmployees: number;
}

export interface EmployeeDashboardData {
    assignedComplaints: number;
    inProgressComplaints: number;
    resolvedComplaints: number;
    escalatedComplaints: number;
    overdueComplaints: number;
}

export interface ComplaintSummary {
    complaintId: number;
    title: string;
    status: string;         
    priority?: string;      
    category?: string;      
    createdAt: string;
    dueDate?: string;
    userName?: string;
}

export interface PagedUserComplaintsResponse {
    items: ComplaintSummary[];
    totalRecords: number;
    pageNumber: number;
    pageSize: number;
}

export interface PagedAdminComplaintsResponse {
    complaints: ComplaintSummary[];
    totalRecords: number;
    pageNumber: number;
    pageSize: number;
}
