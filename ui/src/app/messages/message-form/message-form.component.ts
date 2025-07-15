import { Component, OnInit } from "@angular/core"
import { CommonModule } from "@angular/common" // Import CommonModule
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from "@angular/forms" // Import ReactiveFormsModule
import { RouterModule } from "@angular/router" // Import RouterModule
import { NgSelectModule } from "@ng-select/ng-select" // Import NgSelectModule
import { ToastaService, ToastOptions, ToastaModule } from "ngx-toasta" // Import ToastaModule

import { ActivatedRoute, Router } from "@angular/router"
import { MessageService } from "../../services/message.service"
import { InstanceService } from "../../services/instance.service"
import { GroupService } from "../../services/group.service"
import { CreateMessageWithFileDto, UpdateMessageWithFileDto } from "../../models/message.model"
import { Instance } from "../../models/user.model"
import { Group } from "../../models/message.model"
import { PaginationRequest } from "../../models/common.model"
import { environment } from "../../../environments/environment"

@Component({
  selector: "app-message-form",
  standalone: true, // Đánh dấu là standalone component
  imports: [CommonModule, ReactiveFormsModule, RouterModule, NgSelectModule, ToastaModule], // Import các module cần thiết
  templateUrl: "./message-form.component.html",
  styleUrls: ["./message-form.component.scss"],
})
export class MessageFormComponent implements OnInit {
  messageForm: FormGroup
  messageId: number | null = null
  isEditMode = false
  loading = false
  instances: Instance[] = []
  groups: Group[] = []
  selectedFile: File | null = null
  currentImageUrl: string | null = null
  removeImage = false
  environment = environment

  constructor(
    private fb: FormBuilder,
    private messageService: MessageService,
    private instanceService: InstanceService,
    private groupService: GroupService,
    public router: Router,
    private route: ActivatedRoute,
    private toastaService: ToastaService,
  ) {
    this.messageForm = this.fb.group({
      title: ["", [Validators.required, Validators.maxLength(200)]],
      textContent: ["", Validators.maxLength(4000)],
      messageType: ["Text", Validators.required],
      instanceId: [null, Validators.required],
      groupIds: [[]],
    })
  }

  ngOnInit(): void {
    this.messageId = Number.parseInt(this.route.snapshot.paramMap.get("id") || "0")
    this.isEditMode = !!this.messageId

    this.loadInstances()
    this.loadGroups()

    if (this.isEditMode) {
      this.loadMessage()
    }
  }

  loadMessage(): void {
    this.loading = true
    this.messageService.getMessage(this.messageId!).subscribe({
      next: (message) => {
        this.messageForm.patchValue({
          title: message.title,
          textContent: message.textContent,
          messageType: message.messageType,
          instanceId: message.instanceId,
          groupIds: message.assignedGroups.map((g) => g.id),
        })
        this.currentImageUrl = message.imageUrl || null
        //console.log(this.currentImageUrl)
        this.loading = false
      },
      error: (error) => {
        console.error("Error loading message:", error)
        this.showError("Failed to load message.")
        this.loading = false
        this.router.navigate(["/messages"])
      },
    })
  }

  loadInstances(): void {
    const pagination: PaginationRequest = { page: 1, pageSize: 100 } // Load all for selection
    this.instanceService.getInstances(pagination).subscribe({
      next: (response) => {
        this.instances = response.data
      },
      error: (error) => {
        console.error("Error loading instances:", error)
        this.showError("Failed to load instances.")
      },
    })
  }

  loadGroups(): void {
    const pagination: PaginationRequest = { page: 1, pageSize: 100 } // Load all for selection
    this.groupService.getGroups(pagination).subscribe({
      next: (response) => {
        this.groups = response.data
      },
      error: (error) => {
        console.error("Error loading groups:", error)
        this.showError("Failed to load groups.")
      },
    })
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0]
      this.removeImage = false
      //this.messageForm.get("messageType")?.setValue("Image")
    } else {
      this.selectedFile = null
    }
  }

  onRemoveImageChange(event: Event): void {
    const checkbox = event.target as HTMLInputElement
    this.removeImage = checkbox.checked
    if (this.removeImage) {
      this.selectedFile = null
      this.messageForm.get("messageType")?.setValue("Text") // Default to text if image removed
    }
  }

  onSubmit(): void {
    if (this.messageForm.invalid) {
      this.messageForm.markAllAsTouched()
      this.showError("Please correct the form errors.")
      return
    }

    this.loading = true

    if (this.isEditMode) {
      this.updateMessage()
    } else {
      this.createMessage()
    }
  }

  createMessage(): void {
    const formValue = this.messageForm.value
    const messageData: CreateMessageWithFileDto = {
      title: formValue.title,
      textContent: formValue.textContent,
      messageType: formValue.messageType,
      instanceId: formValue.instanceId,
      imageFile: this.selectedFile || undefined,
      groupIds: formValue.groupIds,
    }

    this.messageService.createMessageWithFile(messageData).subscribe({
      next: () => {
        this.showSuccess("Message created successfully!")
        this.router.navigate(["/messages"])
      },
      error: (error) => {
        console.error("Error creating message:", error)
        this.showError("Failed to create message.")
        this.loading = false
      },
    })
  }

  updateMessage(): void {
    const formValue = this.messageForm.value
    const messageData: UpdateMessageWithFileDto = {
      title: formValue.title,
      textContent: formValue.textContent,
      messageType: formValue.messageType,
      instanceId: formValue.instanceId,
      imageFile: this.selectedFile || undefined,
      removeImage: this.removeImage,
      groupIds: formValue.groupIds,
    }

    this.messageService.updateMessageWithFile(this.messageId!, messageData).subscribe({
      next: () => {
        this.showSuccess("Message updated successfully!")
        this.router.navigate(["/messages"])
      },
      error: (error) => {
        console.error("Error updating message:", error)
        this.showError("Failed to update message.")
        this.loading = false
      },
    })
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.messageForm.get(fieldName)
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
