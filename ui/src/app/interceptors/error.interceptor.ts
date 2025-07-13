import { HttpErrorResponse, HttpInterceptorFn } from "@angular/common/http"
import { inject } from "@angular/core"
import { catchError, throwError } from "rxjs"
import { ToastaService, ToastOptions } from "ngx-toasta"
import { AuthService } from "../services/auth.service"
import { Router } from "@angular/router"

export const ErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const toastaService = inject(ToastaService)
  const authService = inject(AuthService)
  const router = inject(Router)

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = "An unknown error occurred!"

      if (error.error instanceof ErrorEvent) {
        // Client-side errors
        errorMessage = `Error: ${error.error.message}`
      } else {
        // Server-side errors
        switch (error.status) {
          case 400: // Bad Request
            if (error.error && typeof error.error === "object") {
              // Handle validation errors from API (e.g., FluentValidation)
              const validationErrors = Object.values(error.error).flat()
              errorMessage = validationErrors.join("\n")
            } else {
              errorMessage = error.error || "Bad Request"
            }
            break
          case 401: // Unauthorized
            errorMessage = "Unauthorized. Please log in."
            authService.logout() // Log out user on 401
            router.navigate(["/login"])
            break
          case 403: // Forbidden
            errorMessage = "Forbidden. You don't have permission to access this resource."
            break
          case 404: // Not Found
            errorMessage = "Resource not found."
            break
          case 500: // Internal Server Error
            errorMessage = error.error || "Internal Server Error. Please try again later."
            break
          default:
            errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`
            break
        }
      }

      const toastOptions: ToastOptions = {
        title: "Error",
        msg: errorMessage,
        showClose: true,
        timeout: 5000,
        theme: "bootstrap",
      }
      toastaService.error(toastOptions)

      return throwError(() => new Error(errorMessage))
    }),
  )
}
