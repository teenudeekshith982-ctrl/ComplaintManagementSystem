export interface CreateEmployeeRequest {
    name: string;
    email: string;
    password: string;
    phone: string;
    role: string;
    designation: number;
    department: number;
}

export interface EmployeeListItem {
    userId: number;
    name: string;
    email: string;
    phone: string;
    role: string;
    isActive: boolean;
    joinedDate: string;
}

export interface UserListItem {
    userId: number;
    name: string;
    email: string;
    phone: string;
    role: string;
    isActive: boolean;
    joinedDate: string;
}
