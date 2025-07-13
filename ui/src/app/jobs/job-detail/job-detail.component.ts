import { Component, OnInit } from "@angular/core"
import { CommonModule } from "@angular/common" // Import CommonModule
import { RouterModule } from "@angular/router" // Import RouterModule
import { NgbModal, NgbModalModule } from "@ng-bootstrap/ng-bootstrap" // Import NgbModalModule
import { ToastaService, ToastOptions, ToastaModule } from "ngx-toasta" // Import ToastaModule
import { LoadingComponent } from "../../shared/components/loading/loading.component" // Import LoadingComponent
import { ConfirmDialogComponent } from "../../shared/components/confirm-dialog/confirm-dialog.component" // Import ConfirmDialogComponent

import { ActivatedRoute, Router } from "@angular/router"
import { JobService } from "../../services/job.service"
import { Job } from "../../models/job.model"

@Component({
  selector: "app-job-detail",
  standalone: true, // Đánh dấu là standalone component
  imports: [
    CommonModule,
    RouterModule,
    LoadingComponent,
    ConfirmDialogComponent,
    NgbModalModule, // Import NgbModalModule
    ToastaModule, // Import ToastaModule
  ],
  templateUrl: "./job-detail.component.html",
  styleUrls: ["./job-detail.component.scss"],
})
export class JobDetailComponent implements OnInit {
  job: Job | null = null
  loading = true
  jobId: number | null = null

  constructor(
    private route: ActivatedRoute,
    public router: Router,
    private jobService: JobService,
    private modalService: NgbModal,
    private toastaService: ToastaService,
  ) {}

  ngOnInit(): void {
    this.jobId = Number.parseInt(this.route.snapshot.paramMap.get("id") || "0")
    if (this.jobId) {
      this.loadJobDetails()
    } else {
      this.router.navigate(["/jobs"])
    }
  }

  loadJobDetails(): void {
    this.loading = true
    this.jobService.getJob(this.jobId!).subscribe({
      next: (job) => {
        this.job = job
        this.loading = false
      },
      error: (error) => {
        console.error("Error loading job details:", error)
        this.showError("Failed to load job details.")
        this.loading = false
        this.router.navigate(["/jobs"])
      },
    })
  }

  restartJob(): void {
    if (!this.job) return

    const modalRef = this.modalService.open(ConfirmDialogComponent)
    modalRef.componentInstance.title = "Restart Job"
    modalRef.componentInstance.message =
      "Are you sure you want to restart this job? All previous sent messages and logs will be cleared."
    modalRef.componentInstance.confirmText = "Restart"
    modalRef.componentInstance.confirmButtonClass = "btn-warning"

    modalRef.result
      .then((result) => {
        if (result) {
          this.jobService.restartJob(this.jobId!).subscribe({
            next: () => {
              this.showSuccess("Job restarted successfully!")
              this.loadJobDetails()
            },
            error: (error) => {
              console.error("Error restarting job:", error)
            },
          })
        }
      })
      .catch(() => {})
  }

  cancelJob(): void {
    if (!this.job) return

    const modalRef = this.modalService.open(ConfirmDialogComponent)
    modalRef.componentInstance.title = "Cancel Job"
    modalRef.componentInstance.message = "Are you sure you want to cancel this job?"
    modalRef.componentInstance.confirmText = "Cancel"
    modalRef.componentInstance.confirmButtonClass = "btn-danger"

    modalRef.result
      .then((result) => {
        if (result) {
          this.jobService.cancelJob(this.jobId!).subscribe({
            next: () => {
              this.showSuccess("Job cancelled successfully!")
              this.loadJobDetails()
            },
            error: (error) => {
              console.error("Error cancelling job:", error)
            },
          })
        }
      })
      .catch(() => {})
  }

  getJobStatusClass(status: string): string {
    switch (status) {
      case "Pending":
        return "badge bg-warning"
      case "Running":
        return "badge bg-info"
      case "Completed":
        return "badge bg-success"
      case "Failed":
        return "badge bg-danger"
      case "Cancelled":
        return "badge bg-secondary"
      default:
        return "badge bg-secondary"
    }
  }

  getLogLevelClass(level: string): string {
    switch (level) {
      case "Info":
        return "text-info"
      case "Warning":
        return "text-warning"
      case "Error":
        return "text-danger"
      default:
        return ""
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
