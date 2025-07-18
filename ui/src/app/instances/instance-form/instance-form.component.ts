import { Component, OnInit } from "@angular/core"
import { CommonModule } from "@angular/common" // Import CommonModule
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from "@angular/forms" // Import ReactiveFormsModule
import { RouterModule } from "@angular/router" // Import RouterModule
import { ToastaService, ToastOptions, ToastaModule } from "ngx-toasta" // Import ToastaModule

import { ActivatedRoute, Router } from "@angular/router"
import { InstanceService } from "../../services/instance.service"
import { CreateInstanceDto, UpdateInstanceDto } from "../../models/user.model"
import { TranslateModule } from "@ngx-translate/core"

@Component({
  selector: "app-instance-form",
  standalone: true, // Đánh dấu là standalone component
  imports: [CommonModule, ReactiveFormsModule, RouterModule, ToastaModule, TranslateModule], // Import các module cần thiết
  templateUrl: "./instance-form.component.html",
  styleUrls: ["./instance-form.component.scss"],
})
export class InstanceFormComponent implements OnInit {
  instanceForm: FormGroup
  instanceId: number | null = null
  isEditMode = false
  loading = false

  constructor(
    private fb: FormBuilder,
    private instanceService: InstanceService,
    public router: Router,
    private route: ActivatedRoute,
    private toastaService: ToastaService,
  ) {
    this.instanceForm = this.fb.group({
      name: ["", [Validators.required, Validators.maxLength(100)]],
      whatsAppNumber: ["", [Validators.maxLength(50)]],
      whapiToken: ["", [Validators.required, Validators.maxLength(200)]],
      isActive: [true],
    })
  }

  ngOnInit(): void {
    this.instanceId = Number.parseInt(this.route.snapshot.paramMap.get("id") || "0")
    this.isEditMode = !!this.instanceId

    if (this.isEditMode) {
      this.loadInstance()
    }
  }

  loadInstance(): void {
    this.loading = true
    this.instanceService.getInstance(this.instanceId!).subscribe({
      next: (instance) => {
        this.instanceForm.patchValue({
          name: instance.name,
          whatsAppNumber: instance.whatsAppNumber,
          whapiToken: instance.whapiToken,
          isActive: instance.isActive,
        })
        // WhapiToken is not returned by DTO for security, so it won't be pre-filled
        //this.instanceForm.get("whatsAppNumber")?.disable() // WhatsApp number cannot be changed
        this.loading = false
      },
      error: (error) => {
        console.error("Error loading instance:", error)
        this.showError("Failed to load instance.")
        this.loading = false
        this.router.navigate(["/instances"])
      },
    })
  }

  onSubmit(): void {
    if (this.instanceForm.invalid) {
      this.instanceForm.markAllAsTouched()
      this.showError("Please correct the form errors.")
      return
    }

    this.loading = true

    if (this.isEditMode) {
      this.updateInstance()
    } else {
      this.createInstance()
    }
  }

  createInstance(): void {
    const instanceData: CreateInstanceDto = this.instanceForm.value
    this.instanceService.createInstance(instanceData).subscribe({
      next: () => {
        this.showSuccess("Instance created successfully!")
        this.router.navigate(["/instances"])
      },
      error: (error) => {
        console.error("Error creating instance:", error)
        this.showError("Failed to create instance.")
        this.loading = false
      },
    })
  }

  updateInstance(): void {
    const instanceData: UpdateInstanceDto = this.instanceForm.value
    this.instanceService.updateInstance(this.instanceId!, instanceData).subscribe({
      next: () => {
        this.showSuccess("Instance updated successfully!")
        this.router.navigate(["/instances"])
      },
      error: (error) => {
        console.error("Error updating instance:", error)
        this.showError("Failed to update instance.")
        this.loading = false
      },
    })
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.instanceForm.get(fieldName)
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
