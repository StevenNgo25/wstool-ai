import { inject } from "@angular/core"
import { CanActivateFn, Router } from "@angular/router"
import { AuthService } from "../services/auth.service"
import { ToastaService, ToastOptions } from "ngx-toasta"

export const AdminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService)
  const router = inject(Router)
  const toastaService = inject(ToastaService)

  if (authService.isAuthenticated() && authService.isAdmin()) {
    return true
  } else {
    const toastOptions: ToastOptions = {
      title: "Access Denied",
      msg: "You do not have administrative privileges to access this page.",
      showClose: true,
      timeout: 5000,
      theme: "bootstrap",
    }
    toastaService.error(toastOptions)
    router.navigate(["/dashboard"]) // Redirect to dashboard or a forbidden page
    return false
  }
}
