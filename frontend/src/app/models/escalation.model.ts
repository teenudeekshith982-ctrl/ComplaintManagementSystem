export interface CreateEscalationRequest {
    complaintId: number;
    reason: string;
}

export interface EscalationItem {
    escalatedId: number;
    complaintId: number;
    complaintTitle: string;
    department: string;
    departmentId: number;
    assignedTo: string;
    escalatedLevelId: number;
    escalationLevel: string;
    requestedBy: string;
    requestedById: number;
    currentAssignee: string;
    currentAssigneeId: number;
    reason: string;
    escalatedAt: string;
    status: string;
}

export interface EscalationActionRequest {
    action: string;
    employeeId: number;
    comments: string;
}

export interface PagedEscalationResponse {
    data: EscalationItem[];
    totalRecords: number;
    pageNumber: number;
    pageSize: number;
}

export interface EligibleEmployee {
    employeeId: number;
    name: string;
    designation: string;
    departmentName: string;
}

export interface NextLevelResponse {
    maxLevelReached: boolean;
    nextLevel: string | null;
    nextLevelId: number;
}
