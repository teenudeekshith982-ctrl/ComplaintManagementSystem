import { inject } from "@angular/core";
import { CanActivateFn, Router } from "@angular/router";
import { AuthService } from "../Services/auth.service";





export const loginGuard : CanActivateFn = ()=>{

    const authservice = inject(AuthService);
    const router = inject(Router);

    if(authservice.isLoggedIn()){
        router.navigate(['/dashboard']);
        return false;
    }

    return true;


}