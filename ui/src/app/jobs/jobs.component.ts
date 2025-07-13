import { Component, OnInit } from "@angular/core"
import { CommonModule } from "@angular/common" // Import CommonModule
import { FormsModule } from "@angular/forms" // Import FormsModule
import { RouterModule } from "@angular/router" // Import RouterModule
import { NgbModal, NgbModalModule } from "@ng-bootstrap/ng-bootstrap" // Import NgbModalModule
import { ToastaService, ToastOptions, ToastaModule } from "ngx-toasta" // Import ToastaModule

import { Router } from "@angular/router"
import { JobService } from "../services/job.service"
import { Job } from "../models/job.model"
import { PaginationRequest, PaginatedResponse } from "../models/common.model"
import { ConfirmDialogComponent } from "../shared/components/confirm-dialog/confirm-dialog.component"
import { PaginationComponent } from "../shared/components/pagination/pagination.component"
import { LoadingComponent } from "../shared/components/loading/loading.component"

@Component({
  selector: "app-jobs",
  standalone: true, // Đánh dấu là standalone component
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    PaginationComponent,
    LoadingComponent,
    ConfirmDialogComponent,
    NgbModalModule, // Import NgbModalModule
    ToastaModule, // Import ToastaModule
  ],
  templateUrl: "./jobs.component.html",
  styleUrls: ["./jobs.component.scss"],
})
export class JobsComponent implements OnInit {
  jobs: PaginatedResponse<Job> = { data: [], pagination: {} as any }
  loading = true
  searchTerm = ""
  selectedJobs: number[] = []

  paginationRequest: PaginationRequest = {
    page: 1,
    pageSize: 10,
    search: "",
    sortBy: "createdAt",
    sortDirection: "desc",
  }

  constructor(
    private jobService: JobService,
    public router: Router,
    private modalService: NgbModal,
    private toastaService: ToastaService,
  ) {}

  ngOnInit(): void {
    this.loadJobs()
  }

  loadJobs(): void {
    this.loading = true
    this.jobService.getJobs(this.paginationRequest).subscribe({
      next: (response) => {
        this.jobs = response
        this.loading = false
      },
      error: (error) => {
        console.error("Error loading jobs:", error)
        this.loading = false
      },
    })
  }

  search(): void {
    this.paginationRequest.search = this.searchTerm
    this.paginationRequest.page = 1
    this.loadJobs()
  }

  onPageChange(page: number): void {
    this.paginationRequest.page = page
    this.loadJobs()
  }

  onPageSizeChange(pageSize: number): void {
    this.paginationRequest.pageSize = pageSize
    this.paginationRequest.page = 1
    this.loadJobs()
  }

  createJob(): void {
    this.router.navigate(["/jobs/new"])
  }

  viewJobDetails(id: number): void {
    this.router.navigate(["/jobs", id])
  }

  restartJob(id: number): void {
    const modalRef = this.modalService.open(ConfirmDialogComponent)
    modalRef.componentInstance.title = "Restart Job"
    modalRef.componentInstance.message =
      "Are you sure you want to restart this job? All previous sent messages and logs will be cleared."
    modalRef.componentInstance.confirmText = "Restart"
    modalRef.componentInstance.confirmButtonClass = "btn-warning"

    modalRef.result
      .then((result) => {
        if (result) {
          this.jobService.restartJob(id).subscribe({
            next: () => {
              this.showSuccess("Job restarted successfully!")
              this.loadJobs()
            },
            error: (error) => {
              console.error("Error restarting job:", error)
            },
          })
        }
      })
      .catch(() => {})
  }

  cancelJob(id: number): void {
    const modalRef = this.modalService.open(ConfirmDialogComponent)
    modalRef.componentInstance.title = "Cancel Job"
    modalRef.componentInstance.message = "Are you sure you want to cancel this job?"
    modalRef.componentInstance.confirmText = "Cancel"
    modalRef.componentInstance.confirmButtonClass = "btn-danger"

    modalRef.result
      .then((result) => {
        if (result) {
          this.jobService.cancelJob(id).subscribe({
            next: () => {
              this.showSuccess("Job cancelled successfully!")
              this.loadJobs()
            },
            error: (error) => {
              console.error("Error cancelling job:", error)
            },
          })
        }
      })
      .catch(() => {})
  }

  deleteJob(id: number): void {
    const modalRef = this.modalService.open(ConfirmDialogComponent)
    modalRef.componentInstance.title = "Delete Job"
    modalRef.componentInstance.message = "Are you sure you want to delete this job?"
    modalRef.componentInstance.confirmText = "Delete"
    modalRef.componentInstance.confirmButtonClass = "btn-danger"

    modalRef.result
      .then((result) => {
        if (result) {
          this.jobService.deleteJob(id).subscribe({
            next: () => {
              this.showSuccess("Job deleted successfully!")
              this.loadJobs()
            },
            error: (error) => {
              console.error("Error deleting job:", error)
            },
          })
        }
      })
      .catch(() => {})
  }

  toggleSelect(id: number): void {
    const index = this.selectedJobs.indexOf(id)
    if (index > -1) {
      this.selectedJobs.splice(index, 1)
    } else {
      this.selectedJobs.push(id)
    }
  }

  isSelected(id: number): boolean {
    return this.selectedJobs.includes(id)
  }

  toggleSelectAll(): void {
    if (this.isAllSelected()) {
      this.selectedJobs = []
    } else {
      this.selectedJobs = this.jobs.data.map((j) => j.id)
    }
  }

  isAllSelected(): boolean {
    return this.jobs.data.length > 0 && this.selectedJobs.length === this.jobs.data.length
  }

  bulkDelete(): void {
    if (this.selectedJobs.length === 0) return

    const modalRef = this.modalService.open(ConfirmDialogComponent)
    modalRef.componentInstance.title = "Delete Jobs"
    modalRef.componentInstance.message = `Are you sure you want to delete ${this.selectedJobs.length} selected jobs?`
    modalRef.componentInstance.confirmText = "Delete"
    modalRef.componentInstance.confirmButtonClass = "btn-danger"

    modalRef.result
      .then((result) => {
        if (result) {
          this.jobService.bulkDeleteJobs({ jobIds: this.selectedJobs }).subscribe({
            next: () => {
              this.showSuccess("Jobs deleted successfully!")
              this.selectedJobs = []
              this.loadJobs()
            },
            error: (error) => {
              console.error("Error deleting jobs:", error)
            },
          })
        }
      })
      .catch(() => {})
  }

  getJobStatusClass(status: string): string {
    switch (status) {
      case "Pending":
        return "bg-warning"
      case "Running":
        return "bg-info"
      case "Completed":
        return "bg-success"
      case "Failed":
        return "bg-danger"
      case "Cancelled":
        return "bg-secondary"
      default:
        return "bg-secondary"
    }
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
}
