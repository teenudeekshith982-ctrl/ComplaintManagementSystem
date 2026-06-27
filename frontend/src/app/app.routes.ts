import { Routes } from '@angular/router';

import {Login} from './Components/Login/login';
import {Register} from './Components/Register/register';
import { Dashboard } from './Components/Dashboard/dashboard';

export const routes: Routes = [

    {
        path : '',
        component : Login,
        pathMatch : 'full'
    },
    {
        path : 'login',
        component : Login
    },
    {
        path : 'register',
        component : Register
    },
    {
        path : 'dashboard',
        component : Dashboard
    }


];
