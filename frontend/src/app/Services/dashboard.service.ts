import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { UserDashboardData, AdminDashboardData, EmployeeDashboardData, PagedUserComplaintsResponse, PagedAdminComplaintsResponse } from "../models/dashboard.model";

@Injectable({
    providedIn: 'root'
})
export class DashboardService {
    private baseUrl = 'http://localhost:5048/api';

    constructor(private http: HttpClient) { }

    getUserDashboard() {
        return this.http.get<UserDashboardData>(`${this.baseUrl}/Dashboard/user`);
    }

    getAdminDashboard() {
        return this.http.get<AdminDashboardData>(`${this.baseUrl}/Dashboard/admin`);
    }

    getEmployeeDashboard() {
        return this.http.get<EmployeeDashboardData>(`${this.baseUrl}/Dashboard/employee`);
    }

    getUserComplaints(pageNumber = 1, pageSize = 5) {
        let params = new HttpParams()
            .set('pageNumber', pageNumber.toString())
            .set('pageSize', pageSize.toString());
        return this.http.get<PagedUserComplaintsResponse>(`${this.baseUrl}/Complaints/GetUserComplaints`, { params });
    }

    getAllComplaints(pageNumber = 1, pageSize = 5) {
        let params = new HttpParams()
            .set('pageNumber', pageNumber.toString())
            .set('pageSize', pageSize.toString());
        return this.http.get<PagedAdminComplaintsResponse>(`${this.baseUrl}/Complaints`, { params });
    }
}
