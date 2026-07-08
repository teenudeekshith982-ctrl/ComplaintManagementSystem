export interface LoginRequest{
    email : string;
    password : string;
}

export interface LoginResponse{
    token : string;
}

export interface RegisterRequest{
    name : string;
    email : string;
    password : string;
    phone : string;
}

export interface RegisterResponse{
    userId : number;
}

export interface JwtData{
    userId: number; 
    name: string; 
    role: string; 
    employeeId?: number; 
}