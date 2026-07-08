import { inject } from "@angular/core";
import { CanActivateFn, Router } from "@angular/router";
import { AuthService } from "../Services/auth.service";

export const authGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.isLoggedIn()) {
        router.navigate(['/login']);
        return false;
    }

    const user = authService.getUserInfo();
    const expectedRoles = route.data?.['roles'] as string[];

    if (expectedRoles && expectedRoles.length > 0) {
        if (!user || !expectedRoles.includes(user.role)) {
            // Role not authorized - redirect to the user's correct home base dashboard
            if (user?.role === 'Admin') {
                router.navigate(['/admin']);
            } else if (user?.role === 'Employee') {
                router.navigate(['/employee']);
            } else {
                router.navigate(['/dashboard']);
            }
            return false;
        }
    }

    return true;
};