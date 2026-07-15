import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../config';
import { ProfileData, UpdateProfileRequest, ChangePasswordRequest } from '../models/profile.model';

@Injectable({ providedIn: 'root' })
export class ProfileService {
    private baseUrl = `${environment.apiBaseUrl}/api`;

    constructor(private http: HttpClient) {}

    getProfile() {
        return this.http.get<ProfileData>(`${this.baseUrl}/Profile`);
    }

    updateProfile(data: UpdateProfileRequest) {
        return this.http.put<{ message: string; userId: number; name: string; phone: string }>(
            `${this.baseUrl}/Profile`, data
        );
    }

    changePassword(data: ChangePasswordRequest) {
        return this.http.post<{ message: string }>(
            `${this.baseUrl}/Profile/change-password`, data
        );
    }
}
