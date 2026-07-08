export interface CreateComplaintRequest {
    title: string;
    description: string;
    category: number;
}

export interface ComplaintListItem {
    complaintId: number;
    title: string;
    status: string;         // Backend returns status as string enum name (e.g. "Open")
    priority: string;       // Backend returns priority as string (e.g. "Low" or "Not Assigned...")
    category: string;       // Backend returns category as string
    createdAt: string;
    dueDate?: string;
    userName?: string;
    assignedTo?: string;
}

export interface ComplaintDetails {
    title: string;
    description: string;
    priority?: string;
    priorityName?: string;  // Prevents template compilation errors
    status: string;
    assignedto?: string;    // Matches backend property name "Assignedto" (serialized as "assignedto")
    comments: CommentDto[];
    attachments: AttachmentDto[];
    createdAt: string;
    dueDate?: string;
    createdByUserId: number;
    assignedEmployeeId?: number;
    categoryId: number;
    categoryName: string;
}

export interface AttachmentDto {
    attachmentId: number;
    fileName: string;
    filePath: string;
    uploadedAt: string;
}

export interface CommentDto {
    commentId: number;
    commentText: string;
    commentedBy: string;
    createdAt: string;
}

export interface CommentRequest {
    commentText: string;
}

export interface PagedUserComplaintsResponse {
    items: ComplaintListItem[];
    totalRecords: number;
    pageNumber: number;
    pageSize: number;
}

export interface PagedAdminComplaintsResponse {
    complaints: ComplaintListItem[];
    totalRecords: number;
    pageNumber: number;
    pageSize: number;
}
