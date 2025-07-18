import { Component, OnInit } from "@angular/core"
import { CommonModule } from "@angular/common" // Import CommonModule
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from "@angular/forms" // Import ReactiveFormsModule
import { RouterModule } from "@angular/router" // Import RouterModule
import { ToastaService, ToastaConfig, ToastOptions, ToastaModule } from "ngx-toasta" // Import ToastaModule

import { Router } from "@angular/router"
import { AuthService } from "../../services/auth.service"
import { TranslateModule, TranslateService } from "@ngx-translate/core"

@Component({
  selector: "app-login",
  standalone: true, // Đánh dấu là standalone component
  imports: [CommonModule, ReactiveFormsModule, RouterModule, ToastaModule, TranslateModule], // Import các module cần thiết
  templateUrl: "./login.component.html",
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup
  loading = false
  loginFail=false
  selectedLang = 'en';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    public router: Router,
    private toastaService: ToastaService,
    private toastaConfig: ToastaConfig,
    private translate: TranslateService,
  ) {
    this.toastaConfig.theme = "bootstrap"

    this.loginForm = this.fb.group({
      username: ["", Validators.required],
      password: ["", Validators.required],
    })
    this.selectedLang = this.translate.currentLang || 'vi';
  }

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.router.navigate(["/dashboard"])
    }
  }

  onSubmit(): void {
    this.loginFail = false
    if (this.loginForm.valid) {
      this.loading = true
      this.authService.login(this.loginForm.value).subscribe({
        next: (response) => {
          this.loading = false
          this.showSuccess("Login successful!")
          this.router.navigate(["/dashboard"])
        },
        error: (error) => {
          this.loginFail = true
          this.loading = false
          //this.showError("Invalid username or password")
        },
      })
    }
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName)
    return !!(field && field.invalid && (field.dirty || field.touched))
  }

  changeLang(lang: string) {
    this.selectedLang = lang;
    this.translate.use(lang);
    localStorage.setItem('lang', lang);
  }

  private showSuccess(message: string): void {
    const toastOptions: ToastOptions = {
      title: "Success",
      msg: message,
      showClose: true,
      timeout: 3000,
      theme: "bootstrap",
    }
    this.toastaService.success(toastOptions)
  }

  private showError(message: string): void {
    const toastOptions: ToastOptions = {
      title: "Error",
      msg: message,
      showClose: true,
      timeout: 5000,
      theme: "bootstrap",
    }
    this.toastaService.error(toastOptions)
  }
}
