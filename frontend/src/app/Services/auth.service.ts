import { Injectable, signal } from "@angular/core";
import { HttpRequest, HttpHandlerFn, HttpEvent } from "@angular/common/http";
import { JwtData, LoginRequest, LoginResponse, RegisterRequest } from "../models/auth.model";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { Router } from "@angular/router";

export function authInterceptor(req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> {
    const token = localStorage.getItem('token');
    if (token) {
        const cloned = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
        return next(cloned);
    }
    return next(req);
}

@Injectable({
    providedIn: 'root'
})

export class AuthService {

    isLoading = signal(false);

    constructor(private http: HttpClient, private router: Router) { }

    Login(credentials: LoginRequest) {
        return this.http.post<LoginResponse>('http://localhost:5048/api/Auth/Login', credentials);
    }

    Register(credentials: RegisterRequest) {
        return this.http.post('http://localhost:5048/api/Auth/Register', credentials);
    }

    getToken(): string | null {
        return localStorage.getItem('token');
    }

    getUserInfo(): JwtData|null {
        const token = this.getToken();
        if (!token) return null;
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            return {
                userId: payload.sub,
                name: payload.name,
                role: payload.role,
                employeeId: payload.EmployeeId ? Number(payload.EmployeeId) : undefined
            };
        } catch {
            return null;
        }
    }

    logout() {
        localStorage.removeItem('token');
        this.router.navigate(['/login']);
    }

    isLoggedIn(): boolean {
        return !!this.getToken();
    }
}


