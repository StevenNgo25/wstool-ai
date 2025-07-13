import { Component, OnInit } from "@angular/core"
import { CommonModule } from "@angular/common" // Import CommonModule
import { FormsModule } from "@angular/forms" // Import FormsModule
import { RouterModule } from "@angular/router" // Import RouterModule
import { NgbModal, NgbModalModule } from "@ng-bootstrap/ng-bootstrap" // Import NgbModalModule
import { ToastaService, ToastOptions, ToastaModule } from "ngx-toasta" // Import ToastaModule

import { Router } from "@angular/router"
import { UserService } from "../services/user.service"
import { User } from "../models/user.model"
import { PaginationRequest, PaginatedResponse } from "../models/common.model"
import { ConfirmDialogComponent } from "../shared/components/confirm-dialog/confirm-dialog.component"
import { PaginationComponent } from "../shared/components/pagination/pagination.component"
import { LoadingComponent } from "../shared/components/loading/loading.component"

@Component({
  selector: "app-users",
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
  templateUrl: "./users.component.html",
  styleUrls: ["./users.component.scss"],
})
export class UsersComponent implements OnInit {
  users: PaginatedResponse<User> = { data: [], pagination: {} as any }
  loading = true
  searchTerm = ""

  paginationRequest: PaginationRequest = {
    page: 1,
    pageSize: 10,
    search: "",
    sortBy: "",
    sortDirection: "desc",
  }

  constructor(
    private userService: UserService,
    public router: Router,
    private modalService: NgbModal,
    private toastaService: ToastaService,
  ) {}

  ngOnInit(): void {
    this.loadUsers()
  }

  loadUsers(): void {
    this.loading = true
    this.userService.getUsers(this.paginationRequest).subscribe({
      next: (response) => {
        this.users = response
        this.loading = false
      },
      error: (error) => {
        console.error("Error loading users:", error)
        this.loading = false
      },
    })
  }

  search(): void {
    this.paginationRequest.search = this.searchTerm
    this.paginationRequest.page = 1
    this.loadUsers()
  }

  onPageChange(page: number): void {
    this.paginationRequest.page = page
    this.loadUsers()
  }

  onPageSizeChange(pageSize: number): void {
    this.paginationRequest.pageSize = pageSize
    this.paginationRequest.page = 1
    this.loadUsers()
  }

  createUser(): void {
    this.router.navigate(["/users/new"])
  }

  editUser(id: number): void {
    this.router.navigate(["/users/edit", id])
  }

  deleteUser(id: number): void {
    const modalRef = this.modalService.open(ConfirmDialogComponent)
    modalRef.componentInstance.title = "Delete User"
    modalRef.componentInstance.message = "Are you sure you want to delete this user? This action cannot be undone."
    modalRef.componentInstance.confirmText = "Delete"
    modalRef.componentInstance.confirmButtonClass = "btn-danger"

    modalRef.result
      .then((result) => {
        if (result) {
          this.userService.deleteUser(id).subscribe({
            next: () => {
              this.showSuccess("User deleted successfully!")
              this.loadUsers()
            },
            error: (error) => {
              console.error("Error deleting user:", error)
            },
          })
        }
      })
      .catch(() => {})
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
