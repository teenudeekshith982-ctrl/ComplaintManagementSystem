import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../config';
import { Observable } from 'rxjs';
import { AICategoryValidation, AIPriorityRecommendation, AISimilarComplaint } from '../models/ai.model';

@Injectable({
  providedIn: 'root'
})
export class AIService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/api/ai`;

  getCategoryValidation(complaintId: number): Observable<AICategoryValidation> {
    return this.http.get<AICategoryValidation>(`${this.baseUrl}/complaints/${complaintId}/category-validation`);
  }

  getPriorityRecommendation(complaintId: number): Observable<AIPriorityRecommendation> {
    return this.http.get<AIPriorityRecommendation>(`${this.baseUrl}/complaints/${complaintId}/priority-recommendation`);
  }

  getComplaintSummary(complaintId: number): Observable<{ summary: string }> {
    return this.http.get<{ summary: string }>(`${this.baseUrl}/complaints/${complaintId}/summary`);
  }

  getSimilarComplaints(complaintId: number): Observable<AISimilarComplaint[]> {
    return this.http.get<AISimilarComplaint[]>(`${this.baseUrl}/complaints/${complaintId}/similar`);
  }
}
