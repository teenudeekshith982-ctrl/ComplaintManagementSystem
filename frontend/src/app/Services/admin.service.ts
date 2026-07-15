import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../config';
import { CreateEmployeeRequest, UserListItem } from '../models/admin.model';

@Injectable({ providedIn: 'root' })
export class AdminService {
    private baseUrl = `${environment.apiBaseUrl}/api`;

    constructor(private http: HttpClient) {}

    getUsers(pageNumber = 1, pageSize = 10, excludeRole?: string) {
        const excludeParam = excludeRole ? `&excludeRole=${excludeRole}` : '';
        return this.http.get<{ data: UserListItem[]; totalRecords: number; pageNumber: number; pageSize: number }>(
            `${this.baseUrl}/Admin/users?pageNumber=${pageNumber}&pageSize=${pageSize}${excludeParam}`
        );
    }

    createEmployee(data: CreateEmployeeRequest) {
        return this.http.post<{ employeeId: number; userId: number; name: string; email: string; departmentName: string }>(
            `${this.baseUrl}/Admin/employees`, data
        );
    }

    toggleUserActive(userId: number, isActive?: boolean) {
        return this.http.patch<{ message: string }>(
            `${this.baseUrl}/Admin/users/${userId}/toggle-active`, isActive !== undefined ? { isActive } : {}
        );
    }

    getEmployeesByDepartment(departmentId?: number) {
        const params = departmentId ? `?departmentId=${departmentId}` : '';
        return this.http.get<{ employeeId: number; employeeName: string; departmentName: string; designation: string }[]>(
            `${this.baseUrl}/MasterData/employees${params}`
        );
    }
}
