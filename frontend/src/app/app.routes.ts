import { Routes } from '@angular/router';

import { Login } from './Components/Login/login';
import { Register } from './Components/Register/register';
import { UserDashboard } from './Components/user-dashboard/user-dashboard';
import { AdminDashboard } from './Components/admin-dashboard/admin-dashboard';
import { EmployeeDashboard } from './Components/employee-dashboard/employee-dashboard';
import { ComplaintList } from './Components/complaint-list/complaint-list';
import { ComplaintForm } from './Components/complaint-form/complaint-form';
import { Profile } from './Components/profile/profile';
import { AdminComplaints } from './Components/admin-complaints/admin-complaints';
import { AdminEmployees } from './Components/admin-employees/admin-employees';
import { AdminEmployeeForm } from './Components/admin-employee-form/admin-employee-form';
import { AdminEscalations } from './Components/admin-escalations/admin-escalations';
import { AdminReports } from './Components/admin-reports/admin-reports';
import { EmployeeComplaints } from './Components/employee-complaints/employee-complaints';
import { EmployeeEscalate } from './Components/employee-escalate/employee-escalate';
import { ComplaintDetailsComponent } from './Components/complaint-details/complaint-details';
import { loginGuard } from './Guards/login.guard';
import { authGuard } from './Guards/auth.guard';

export const routes: Routes = [
    {
        path : '',
        component : Login,
        pathMatch : 'full'
    },
    {
        path : 'login',
        component : Login,
        canActivate:[loginGuard]
    },
    {
        path : 'register',
        component : Register,
        canActivate:[loginGuard]
    },
    {
        path : 'dashboard',
        component : UserDashboard,
        canActivate : [authGuard],
        data: { roles: ['User'] }
    },
    {
        path : 'complaints',
        component : ComplaintList,
        canActivate : [authGuard],
        data: { roles: ['User'] }
    },
    {
        path : 'complaints/new',
        component : ComplaintForm,
        canActivate : [authGuard],
        data: { roles: ['User'] }
    },
    {
        path : 'complaints/:id',
        component : ComplaintDetailsComponent,
        canActivate : [authGuard],
        data: { roles: ['User', 'Admin', 'Employee'] }
    },
    {
        path : 'profile',
        component : Profile,
        canActivate : [authGuard]
    },
    {
        path : 'admin',
        component : AdminDashboard,
        canActivate : [authGuard],
        data: { roles: ['Admin'] }
    },
    {
        path : 'admin/complaints',
        component : AdminComplaints,
        canActivate : [authGuard],
        data: { roles: ['Admin'] }
    },
    {
        path : 'admin/employees',
        component : AdminEmployees,
        canActivate : [authGuard],
        data: { roles: ['Admin'] }
    },
    {
        path : 'admin/employees/new',
        component : AdminEmployeeForm,
        canActivate : [authGuard],
        data: { roles: ['Admin'] }
    },
    {
        path : 'admin/escalations',
        component : AdminEscalations,
        canActivate : [authGuard],
        data: { roles: ['Admin'] }
    },
    {
        path : 'admin/reports',
        component : AdminReports,
        canActivate : [authGuard],
        data: { roles: ['Admin'] }
    },
    {
        path : 'employee',
        component : EmployeeDashboard,
        canActivate : [authGuard],
        data: { roles: ['Employee'] }
    },
    {
        path : 'employee/complaints',
        component : EmployeeComplaints,
        canActivate : [authGuard],
        data: { roles: ['Employee'] }
    },
    {
        path : 'employee/escalate',
        component : EmployeeEscalate,
        canActivate : [authGuard],
        data: { roles: ['Employee'] }
    }
];
