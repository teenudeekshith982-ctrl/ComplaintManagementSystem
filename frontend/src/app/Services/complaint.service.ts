import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { ComplaintDetails, CommentRequest, PagedUserComplaintsResponse, PagedAdminComplaintsResponse } from '../models/complaint.model';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ComplaintService {
    private baseUrl = 'http://localhost:5048/api';

    constructor(private http: HttpClient) {}

    getUserComplaints(filters: {
        pageNumber?: number;
        pageSize?: number;
        status?: number;
        category?: number;
        fromDate?: string;
        toDate?: string;
        searchTerm?: string;
    } = {}): Observable<PagedUserComplaintsResponse> {
        let params = new HttpParams();
        params = params.set('pageNumber', (filters.pageNumber ?? 1).toString());
        params = params.set('pageSize', (filters.pageSize ?? 10).toString());
        
        if (filters.status !== undefined && filters.status !== null) {
            params = params.set('status', filters.status.toString());
        }
        if (filters.category !== undefined && filters.category !== null) {
            params = params.set('category', filters.category.toString());
        }
        if (filters.fromDate) {
            params = params.set('fromDate', filters.fromDate);
        }
        if (filters.toDate) {
            params = params.set('toDate', filters.toDate);
        }
        if (filters.searchTerm) {
            params = params.set('searchTerm', filters.searchTerm);
        }

        return this.http.get<PagedUserComplaintsResponse>(`${this.baseUrl}/Complaints/GetUserComplaints`, { params });
    }

    getAllComplaints(filters: {
        pageNumber?: number;
        pageSize?: number;
        status?: number;
        priority?: number;
        category?: number;
        fromDate?: string;
        toDate?: string;
        searchTerm?: string;
    } = {}): Observable<PagedAdminComplaintsResponse> {
        let params = new HttpParams();
        params = params.set('pageNumber', (filters.pageNumber ?? 1).toString());
        params = params.set('pageSize', (filters.pageSize ?? 10).toString());
        
        if (filters.status !== undefined && filters.status !== null) {
            params = params.set('status', filters.status.toString());
        }
        if (filters.priority !== undefined && filters.priority !== null) {
            params = params.set('priority', filters.priority.toString());
        }
        if (filters.category !== undefined && filters.category !== null) {
            params = params.set('category', filters.category.toString());
        }
        if (filters.fromDate) {
            params = params.set('fromDate', filters.fromDate);
        }
        if (filters.toDate) {
            params = params.set('toDate', filters.toDate);
        }
        if (filters.searchTerm) {
            params = params.set('searchTerm', filters.searchTerm);
        }

        return this.http.get<PagedAdminComplaintsResponse>(`${this.baseUrl}/Complaints`, { params });
    }

    getComplaintDetails(id: number): Observable<ComplaintDetails> {
        return this.http.get<ComplaintDetails>(`${this.baseUrl}/Complaints/${id}/complaintdetails`);
    }

    createComplaint(title: string, description: string, category: number, attachments?: File[]) {
        const formData = new FormData();
        formData.append('title', title);
        formData.append('description', description);
        formData.append('category', category.toString());
        
        if (attachments && attachments.length > 0) {
            for (let i = 0; i < attachments.length; i++) {
                formData.append('attachments', attachments[i], attachments[i].name);
            }
        }

        return this.http.post<{ complaintId: number; title: string; status: string; createdAt: string }>(
            `${this.baseUrl}/Complaints`,
            formData
        );
    }

    updateStatus(id: number, status: number) {
        return this.http.patch<{ message: string }>(
            `${this.baseUrl}/Complaints/${id}/status`,
            { status }
        );
    }

    addComment(id: number, data: CommentRequest) {
        return this.http.post<{ commentId: number; commentText: string; commentedBy: string; createdAt: string }>(
            `${this.baseUrl}/Complaints/${id}/comments`,
            data
        );
    }

    assignPriority(id: number, priority: number) {
        return this.http.patch<{ message: string }>(
            `${this.baseUrl}/Complaints/${id}/priority`,
            { priority }
        );
    }

    assignComplaint(id: number) {
        return this.http.patch<{ isAssigned: boolean; message: string; employees?: { employeeId: number; employeeName: string }[] }>(
            `${this.baseUrl}/Complaints/${id}/assign`,
            {}
        );
    }

    assignComplaintToEmployee(id: number, employeeId: number) {
        return this.http.patch<{ message: string }>(
            `${this.baseUrl}/Complaints/${id}/assign/${employeeId}`,
            {}
        );
    }

    getComplaintTracking(id: number) {
        return this.http.get<{ action: string; description: string; performedBy: string; createdAt: string }[]>(
            `${this.baseUrl}/Complaints/${id}/tracking`
        );
    }

    downloadAttachment(complaintId: number, attachmentId: number): Observable<Blob> {
        return this.http.get(`${this.baseUrl}/Complaints/${complaintId}/attachments/${attachmentId}/download`, {
            responseType: 'blob'
        });
    }
}
