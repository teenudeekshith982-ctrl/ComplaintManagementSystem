import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { ToastService } from '../Services/toast.service';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toastService = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unexpected error occurred';

      if (error.status === 0) {
        errorMessage = 'Cannot connect to the server. Please check your internet connection or backend server status.';
      } else if (error.status === 401) {
        if (req.url.includes('/Auth/Login')) {
          errorMessage = 'Invalid email or password.';
        } else {
          errorMessage = 'Session expired. Please login again.';
        }
      } else if (error.status === 403) {
        errorMessage = 'Forbidden. You do not have permission to perform this action.';
      } else if (error.error) {
        if (typeof error.error === 'object') {
          if (error.error.errors) {
            // Handle validation errors from ASP.NET Core ModelState
            const valErrors: string[] = [];
            for (const key of Object.keys(error.error.errors)) {
              const errs = error.error.errors[key];
              if (Array.isArray(errs)) {
                valErrors.push(...errs);
              } else {
                valErrors.push(errs);
              }
            }
            errorMessage = valErrors.join('\n');
          } else if (error.error.message) {
            errorMessage = error.error.message;
          } else if (error.error.title) {
            errorMessage = error.error.title;
          }
        } else if (typeof error.error === 'string') {
          const trimmed = error.error.trim();
          if (trimmed.startsWith('<html') || trimmed.startsWith('<!DOCTYPE') || trimmed.startsWith('<')) {
            errorMessage = 'An unexpected server error occurred. Please try again later.';
          } else {
            errorMessage = error.error;
          }
        }
      } else if (error.message) {
        errorMessage = error.message;
      }

      toastService.error(errorMessage);
      return throwError(() => error);
    })
  );
};
