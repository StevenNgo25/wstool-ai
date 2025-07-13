import { Component, OnInit } from "@angular/core"
import { CommonModule } from "@angular/common" // Import CommonModule
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from "@angular/forms" // Import ReactiveFormsModule
import { RouterModule } from "@angular/router" // Import RouterModule
import { NgSelectModule } from "@ng-select/ng-select" // Import NgSelectModule
import { ToastaService, ToastOptions, ToastaModule } from "ngx-toasta" // Import ToastaModule

import { ActivatedRoute, Router } from "@angular/router"
import { UserService } from "../../services/user.service"
import { InstanceService } from "../../services/instance.service"
import { CreateUserDto, UpdateUserDto, Instance, AssignInstancesDto } from "../../models/user.model"
import { PaginationRequest } from "../../models/common.model"

@Component({
  selector: "app-user-form",
  standalone: true, // Đánh dấu là standalone component
  imports: [CommonModule, ReactiveFormsModule, RouterModule, NgSelectModule, ToastaModule], // Import các module cần thiết
  templateUrl: "./user-form.component.html",
  styleUrls: ["./user-form.component.scss"],
})
export class UserFormComponent implements OnInit {
  userForm: FormGroup
  userId: number | null = null
  isEditMode = false
  loading = false
  allInstances: Instance[] = []
  assignedInstanceIds: number[] = []

  userRoles = ["Admin", "Member"]

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private instanceService: InstanceService,
    public router: Router,
    private route: ActivatedRoute,
    private toastaService: ToastaService,
  ) {
    this.userForm = this.fb.group({
      username: ["", [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      email: ["", [Validators.required, Validators.email, Validators.maxLength(200)]],
      password: ["", [Validators.minLength(6), Validators.maxLength(100)]],
      role: ["Member", Validators.required],
      isActive: [true],
      assignedInstances: [[]], // For ng-select
      canSendMessages: [true],
      canCreateJobs: [true],
    })
  }

  ngOnInit(): void {
    this.userId = Number.parseInt(this.route.snapshot.paramMap.get("id") || "0")
    this.isEditMode = !!this.userId

    this.loadAllInstances()

    if (this.isEditMode) {
      this.loadUser()
      this.userForm.get("password")?.setValidators(null) // Password not required for edit
      this.userForm.get("password")?.updateValueAndValidity()
    } else {
      this.userForm.get("password")?.setValidators(Validators.required)
      this.userForm.get("password")?.updateValueAndValidity()
    }
  }

  loadUser(): void {
    this.loading = true
    this.userService.getUser(this.userId!).subscribe({
      next: (user) => {
        this.userForm.patchValue({
          username: user.username,
          email: user.email,
          role: user.role,
          isActive: user.isActive,
          // Password is not patched for security
        })
        this.assignedInstanceIds = user.assignedInstances.map((i) => i.id)
        this.userForm.get("assignedInstances")?.setValue(this.assignedInstanceIds)

        // Set permissions from the first assigned instance if available, or default
        if (user.assignedInstances.length > 0) {
          this.userService.getUserInstances(this.userId!).subscribe((instances) => {
            const firstInstance = instances[0] // Assuming permissions are consistent across instances for a user
            if (firstInstance) {
              // This part needs to be handled carefully as AppUserInstance DTO is not exposed
              // For now, we'll assume default true or fetch specific AppUserInstance if needed
              // For simplicity, we'll just use the default values from the form.
            }
          })
        }
        this.loading = false
      },
      error: (error) => {
        console.error("Error loading user:", error)
        this.showError("Failed to load user.")
        this.loading = false
        this.router.navigate(["/users"])
      },
    })
  }

  loadAllInstances(): void {
    const pagination: PaginationRequest = { page: 1, pageSize: 100 } // Load all for selection
    this.instanceService.getInstances(pagination).subscribe({
      next: (response) => {
        this.allInstances = response.data
      },
      error: (error) => {
        console.error("Error loading instances:", error)
        this.showError("Failed to load instances for assignment.")
      },
    })
  }

  onSubmit(): void {
    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched()
      this.showError("Please correct the form errors.")
      return
    }

    this.loading = true

    if (this.isEditMode) {
      this.updateUser()
    } else {
      this.createUser()
    }
  }

  createUser(): void {
    const userData: CreateUserDto = this.userForm.value
    this.userService.createUser(userData).subscribe({
      next: (user) => {
        this.showSuccess("User created successfully!")
        if (this.userForm.value.assignedInstances.length > 0) {
          this.assignInstancesToUser(user.id)
        } else {
          this.router.navigate(["/users"])
        }
      },
      error: (error) => {
        console.error("Error creating user:", error)
        this.showError("Failed to create user.")
        this.loading = false
      },
    })
  }

  updateUser(): void {
    const userData: UpdateUserDto = this.userForm.value
    this.userService.updateUser(this.userId!, userData).subscribe({
      next: () => {
        this.showSuccess("User updated successfully!")
        this.assignInstancesToUser(this.userId!)
      },
      error: (error) => {
        console.error("Error updating user:", error)
        this.showError("Failed to update user.")
        this.loading = false
      },
    })
  }

  assignInstancesToUser(userId: number): void {
    const assignedInstanceIds = this.userForm.value.assignedInstances
    const assignmentData: AssignInstancesDto = {
      instanceIds: assignedInstanceIds,
      canSendMessages: this.userForm.value.canSendMessages,
      canCreateJobs: this.userForm.value.canCreateJobs,
    }

    this.userService.assignInstances(userId, assignmentData).subscribe({
      next: () => {
        this.showSuccess("Instances assigned successfully!")
        this.router.navigate(["/users"])
      },
      error: (error) => {
        console.error("Error assigning instances:", error)
        this.showError("Failed to assign instances.")
        this.loading = false
      },
    })
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.userForm.get(fieldName)
    return !!(field && field.invalid && (field.dirty || field.touched))
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
