import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DashboardSummaryResponse {
    averageResolutionTimeHours: number;
    slaBreachRate: number;
    unassignedTicketsCount: number;
    openEscalationsCount: number;
}

export interface ChartDataPoint {
    name: string;
    value: number;
}

@Injectable({ providedIn: 'root' })
export class AnalyticsService {
    private baseUrl = 'http://localhost:5048/api/Analytics';

    constructor(private http: HttpClient) {}

    getDashboardSummary(): Observable<DashboardSummaryResponse> {
        return this.http.get<DashboardSummaryResponse>(`${this.baseUrl}/dashboard`);
    }

    getComplaintsByStatus(): Observable<Record<string, number>> {
        return this.http.get<Record<string, number>>(`${this.baseUrl}/complaints-by-status`);
    }

    getComplaintsByCategory(): Observable<Record<string, number>> {
        return this.http.get<Record<string, number>>(`${this.baseUrl}/complaints-by-category`);
    }

    getMonthlyTrend(monthsCount = 6): Observable<Record<string, number>> {
        return this.http.get<Record<string, number>>(`${this.baseUrl}/monthly-trend?monthsCount=${monthsCount}`);
    }
}
