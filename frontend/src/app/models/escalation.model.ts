export interface CreateEscalationRequest {
    complaintId: number;
    escalationLevel: number;
    reason: string;
}

export interface EscalationItem {
    escalatedId: number;
    complaintId: number;
    complaintTitle: string;
    department: string;
    assignedTo: string;
    escalationLevel: string;
    reason: string;
    escalatedAt: string;
}

export interface EscalationActionRequest {
    action: string;
    comments?: string;
}

export interface PagedEscalationResponse {
    data: EscalationItem[];
    totalRecords: number;
    pageNumber: number;
    pageSize: number;
}
