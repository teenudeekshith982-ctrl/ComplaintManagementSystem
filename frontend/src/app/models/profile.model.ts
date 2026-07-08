export interface ProfileData {
    userId: number;
    name: string;
    email: string;
    phone: string;
    role: string;
    joinedDate: string;
    employeeInfo?: EmployeeInfo;
}

export interface EmployeeInfo {
    employeeId: number;
    departmentId: number;
    departmentName: string;
    designation: string;
}

export interface UpdateProfileRequest {
    name: string;
    phone: string;
}

export interface ChangePasswordRequest {
    currentPassword: string;
    newPassword: string;
    confirmPassword: string;
}
