import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../config';
import { Observable } from 'rxjs';
import { DashboardSummaryResponse } from '../models/DashboardSummaryResponse.model';
import { ComplaintStatusAnalytics } from '../models/ComplaintStatusAnalytics.model';
import { ComplaintCategoryAnalytics } from '../models/ComplaintCategoryAnalytics.model';
import { MonthlyTrend } from '../models/MonthlyTrend.model';



@Injectable({ providedIn: 'root' })
export class AnalyticsService {
    private baseUrl = `${environment.apiBaseUrl}/api/Analytics`;

    constructor(private http: HttpClient) {}

    getDashboardSummary(): Observable<DashboardSummaryResponse> {
        return this.http.get<DashboardSummaryResponse>(`${this.baseUrl}/dashboard`);
    }

    getComplaintsByStatus(): Observable<ComplaintStatusAnalytics[]> {
        return this.http.get<ComplaintStatusAnalytics[]>(`${this.baseUrl}/complaints-by-status`);
    }

    getComplaintsByCategory(): Observable<ComplaintCategoryAnalytics[]> {
        return this.http.get<ComplaintCategoryAnalytics[]>(`${this.baseUrl}/complaints-by-category`);
    }

    getMonthlyTrend(monthsCount = 4): Observable<MonthlyTrend[]> {
        return this.http.get<MonthlyTrend[]>(`${this.baseUrl}/monthly-trend?monthsCount=${monthsCount}`);
    }
}
