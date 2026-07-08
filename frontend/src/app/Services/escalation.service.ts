import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { CreateEscalationRequest, EscalationItem, EscalationActionRequest, PagedEscalationResponse } from '../models/escalation.model';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class EscalationService {
    private baseUrl = 'http://localhost:5048/api';

    constructor(private http: HttpClient) {}

    getEscalations(pageNumber = 1, pageSize = 10): Observable<PagedEscalationResponse> {
        let params = new HttpParams()
            .set('pageNumber', pageNumber.toString())
            .set('pageSize', pageSize.toString());
            
        return this.http.get<PagedEscalationResponse>(
            `${this.baseUrl}/Escalation`, { params }
        );
    }

    getComplaintEscalations(complaintId: number): Observable<EscalationItem[]> {
        return this.http.get<EscalationItem[]>(
            `${this.baseUrl}/Escalation/complaint/${complaintId}`
        );
    }

    createEscalation(data: CreateEscalationRequest) {
        return this.http.post<{ escalationId: number; complaintId: number; escalationLevel: string; reason: string; escalatedAt: string }>(
            `${this.baseUrl}/Escalation`, data
        );
    }

    resolveEscalation(id: number, action: string, comments: string) {
        return this.http.post<{ message: string }>(
            `${this.baseUrl}/Escalation/${id}/action`, { action, comments }
        );
    }
}
