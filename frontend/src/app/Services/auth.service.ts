import { Injectable, signal } from "@angular/core";
import { LoginRequest, RegisterRequest } from "../models/auth.model";
import { HttpClient } from "@angular/common/http";

@Injectable({
    providedIn: 'root'
})

export class AuthService {

    isLoading = signal(false);

    constructor(private http: HttpClient) { }


    Login(credentials: LoginRequest) {
        return this.http.post('http://localhost:5048/api/Auth/Login', credentials);
    }

    Register(credentials : RegisterRequest) {
        return this.http.post('http://localhost:5048/api/Auth/Register', credentials);
    }
}


