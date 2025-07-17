import { Component, OnInit, TemplateRef, ViewChild } from "@angular/core"
import { CommonModule } from "@angular/common" // Import CommonModule
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from "@angular/forms" // Import ReactiveFormsModule
import { RouterModule } from "@angular/router" // Import RouterModule
import { NgSelectModule } from "@ng-select/ng-select" // Import NgSelectModule
import { ToastaService, ToastOptions, ToastaModule } from "ngx-toasta" // Import ToastaModule

import { Router } from "@angular/router"
import { JobService } from "../../services/job.service"
import { MessageService } from "../../services/message.service"
import { InstanceService } from "../../services/instance.service"
import { GroupService } from "../../services/group.service"
import { CreateJobDto } from "../../models/job.model"
import { Message } from "../../models/message.model"
import { Instance } from "../../models/user.model"
import { Group } from "../../models/message.model"
import { PaginationRequest } from "../../models/common.model"
import { NgbModal, NgbModalRef } from "@ng-bootstrap/ng-bootstrap"

@Component({
  selector: "app-job-form",
  standalone: true, // Đánh dấu là standalone component
  imports: [CommonModule, ReactiveFormsModule, RouterModule, NgSelectModule, ToastaModule, FormsModule], // Import các module cần thiết
  templateUrl: "./job-form.component.html",
  styleUrls: ["./job-form.component.scss"],
})
export class JobFormComponent implements OnInit {
  jobForm: FormGroup
  loading = false
  messages: Message[] = []
  instances: Instance[] = []
  groups: Group[] = []
  
  @ViewChild('groupModal') groupModal!: TemplateRef<any>;
  groupSearch: string = '';
  selectedGroupIds: any[] = [];
  selectedGroups: Group[] = [];
  selectAllChecked = false;

  jobTypes = [
    { value: "SendToGroups", label: "Send to Groups" },
    { value: "SendToUsers", label: "Send to Users" },
  ]

  constructor(
    private fb: FormBuilder,
    private jobService: JobService,
    private messageService: MessageService,
    private instanceService: InstanceService,
    private groupService: GroupService,
    public router: Router,
    private toastaService: ToastaService,
    private modalService: NgbModal,
  ) {
    this.jobForm = this.fb.group({
      jobType: ["SendToGroups", Validators.required],
      messageId: [null, Validators.required],
      instanceId: [null, Validators.required],
      scheduledAt: [null],
      groupIds: [[]],
      //phoneNumbers: [""], // Comma-separated string for phone numbers
    })

    // Listen for jobType changes to update validators
    this.jobForm.get("jobType")?.valueChanges.subscribe((type) => {
      const groupIdsControl = this.jobForm.get("groupIds")
      //const phoneNumbersControl = this.jobForm.get("phoneNumbers")

      if (type === "SendToGroups") {
        groupIdsControl?.setValidators(Validators.required)
        //phoneNumbersControl?.clearValidators()
      } else {
        groupIdsControl?.clearValidators()
        //phoneNumbersControl?.setValidators(Validators.required)
      }
      groupIdsControl?.updateValueAndValidity()
      //phoneNumbersControl?.updateValueAndValidity()
    })
  }

  ngOnInit(): void {
    this.loadMessages()
    this.loadInstances()
    this.loadGroups()
  }

  loadMessages(): void {
    const pagination: PaginationRequest = { page: 1, pageSize: 1000 }
    this.messageService.getMessages(pagination).subscribe({
      next: (response) => {
        this.messages = response.data
      },
      error: (error) => {
        console.error("Error loading messages:", error)
        this.showError("Failed to load messages.")
      },
    })
  }

  loadInstances(): void {
    const pagination: PaginationRequest = { page: 1, pageSize: 1000 }
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
    const pagination: PaginationRequest = { page: 1, pageSize: 1000 }
    this.groupService.searchGroups(pagination, this.jobForm.get("instanceId")?.value ? [this.jobForm.get("instanceId")?.value] : []).subscribe({
      next: (response) => {
        this.groups = response.data
      },
      error: (error) => {
        console.error("Error loading groups:", error)
        this.showError("Failed to load groups.")
      },
    })
  }

  onSubmit(): void {
    if (this.jobForm.invalid) {
      this.jobForm.markAllAsTouched()
      this.showError("Please correct the form errors.")
      return
    }

    this.loading = true
    const formValue = this.jobForm.value

    const jobData: CreateJobDto = {
      jobType: formValue.jobType,
      messageId: formValue.messageId,
      instanceId: formValue.instanceId,
      scheduledAt: formValue.scheduledAt ? new Date(formValue.scheduledAt).toISOString() : undefined,
    }

    //if (formValue.jobType === "SendToGroups") {
      jobData.groupIds = formValue.groupIds
    /* } else if (formValue.jobType === "SendToUsers") {
      jobData.phoneNumbers = formValue.phoneNumbers
        .split(",")
        .map((s: string) => s.trim())
        .filter((s: string) => s.length > 0)
    } */

    this.jobService.createJob(jobData).subscribe({
      next: () => {
        this.showSuccess("Job created successfully!")
        this.router.navigate(["/jobs"])
      },
      error: (error) => {
        console.error("Error creating job:", error)
        this.showError("Failed to create job.")
        this.loading = false
      },
    })
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.jobForm.get(fieldName)
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

  

  toggleSelectAllGroups(): void {
    const groupIdsControl = this.jobForm.get("groupIds")
    if (!groupIdsControl) return

    const allFilteredGroupIds = this.groups.map((g) => g.id)
    const currentSelected = groupIdsControl.value as number[]

    if (
      currentSelected.length === allFilteredGroupIds.length &&
      currentSelected.every((id) => allFilteredGroupIds.includes(id))
    ) {
      // All are selected, deselect all
      groupIdsControl.setValue([])
    } else {
      // Not all are selected, select all
      groupIdsControl.setValue(allFilteredGroupIds)
    }
  }

  onInstanceChange($event: any) {
    console.log($event)
    this.loadGroups()
  }

  openGroupModal() {
    this.groupSearch = '';
    this.selectAllChecked = this.selectedGroupIds.length === this.groups.length;
    this.modalService.open(this.groupModal, { size: 'lg' });
  }
  filteredGroups() {
    if (!this.groupSearch) return this.groups;
    return this.groups.filter(g =>
      g.name.toLowerCase().includes(this.groupSearch.toLowerCase())
    );
  }
  toggleGroupSelection(group: any) {
    const index = this.selectedGroupIds.indexOf(group.id);
    if (index > -1) {
      this.selectedGroupIds.splice(index, 1);
    } else {
      this.selectedGroupIds.push(group.id);
    }
  
    this.selectAllChecked = this.selectedGroupIds.length === this.groups.length;
  }
  toggleSelectAll() {
  const filtered = this.filteredGroups();
  const filteredIds = filtered.map(g => g.id);

  if (this.selectAllChecked) {
    // Thêm chỉ các nhóm đang hiển thị nếu chưa có
    this.selectedGroupIds = Array.from(new Set([...this.selectedGroupIds, ...filteredIds]));
  } else {
    // Loại bỏ các nhóm đang hiển thị khỏi danh sách chọn
    this.selectedGroupIds = this.selectedGroupIds.filter(id => !filteredIds.includes(id));
  }
}

  confirmGroupSelection(modalRef: NgbModalRef) {
    this.selectedGroups = this.groups.filter(g => this.selectedGroupIds.includes(g.id));
    this.jobForm.get('groupIds')?.setValue(this.selectedGroupIds);
    modalRef.close();
  }
  isGroupSelected(id: any): boolean {
    return this.selectedGroupIds.includes(id);
  }
  
}
