import { Component, OnInit } from "@angular/core"
import { CommonModule } from "@angular/common" // Import CommonModule
import { FormsModule } from "@angular/forms" // Import FormsModule
import { RouterModule } from "@angular/router" // Import RouterModule
import { NgbModal, NgbModalModule } from "@ng-bootstrap/ng-bootstrap" // Import NgbModalModule
import { ToastaService, ToastaConfig, ToastOptions, ToastaModule } from "ngx-toasta" // Import ToastaModule

import { Router } from "@angular/router"
import { MessageService } from "../services/message.service"
import { Message } from "../models/message.model"
import { PaginationRequest, PaginatedResponse } from "../models/common.model"
import { ConfirmDialogComponent } from "../shared/components/confirm-dialog/confirm-dialog.component"
import { PaginationComponent } from "../shared/components/pagination/pagination.component"
import { LoadingComponent } from "../shared/components/loading/loading.component"

@Component({
  selector: "app-messages",
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
  templateUrl: "./messages.component.html",
})
export class MessagesComponent implements OnInit {
  messages: PaginatedResponse<Message> = { data: [], pagination: {} as any }
  loading = true
  searchTerm = ""
  selectedMessages: number[] = []

  paginationRequest: PaginationRequest = {
    page: 1,
    pageSize: 10,
    search: "",
    sortBy: "createdAt",
    sortDirection: "desc",
  }

  constructor(
    private messageService: MessageService,
    public router: Router,
    private modalService: NgbModal,
    private toastaService: ToastaService,
    private toastaConfig: ToastaConfig,
  ) {
    this.toastaConfig.theme = "bootstrap"
  }

  ngOnInit(): void {
    this.loadMessages()
  }

  loadMessages(): void {
    this.loading = true
    this.messageService.getMessages(this.paginationRequest).subscribe({
      next: (response) => {
        this.messages = response
        this.loading = false
      },
      error: (error) => {
        console.error("Error loading messages:", error)
        this.loading = false
      },
    })
  }

  search(): void {
    this.paginationRequest.search = this.searchTerm
    this.paginationRequest.page = 1
    this.loadMessages()
  }

  onPageChange(page: number): void {
    this.paginationRequest.page = page
    this.loadMessages()
  }

  onPageSizeChange(pageSize: number): void {
    this.paginationRequest.pageSize = pageSize
    this.paginationRequest.page = 1
    this.loadMessages()
  }

  createMessage(): void {
    this.router.navigate(["/messages/new"])
  }

  editMessage(id: number): void {
    this.router.navigate(["/messages/edit", id])
  }

  deleteMessage(id: number): void {
    const modalRef = this.modalService.open(ConfirmDialogComponent)
    modalRef.componentInstance.title = "Delete Message"
    modalRef.componentInstance.message = "Are you sure you want to delete this message?"
    modalRef.componentInstance.confirmText = "Delete"
    modalRef.componentInstance.confirmButtonClass = "btn-danger"

    modalRef.result
      .then((result) => {
        if (result) {
          this.messageService.deleteMessage(id).subscribe({
            next: () => {
              this.showSuccess("Message deleted successfully")
              this.loadMessages()
            },
            error: (error) => {
              console.error("Error deleting message:", error)
            },
          })
        }
      })
      .catch(() => {})
  }

  toggleSelect(id: number): void {
    const index = this.selectedMessages.indexOf(id)
    if (index > -1) {
      this.selectedMessages.splice(index, 1)
    } else {
      this.selectedMessages.push(id)
    }
  }

  isSelected(id: number): boolean {
    return this.selectedMessages.includes(id)
  }

  toggleSelectAll(): void {
    if (this.isAllSelected()) {
      this.selectedMessages = []
    } else {
      this.selectedMessages = this.messages.data.map((m) => m.id)
    }
  }

  isAllSelected(): boolean {
    return this.messages.data.length > 0 && this.selectedMessages.length === this.messages.data.length
  }

  bulkDelete(): void {
    if (this.selectedMessages.length === 0) return

    const modalRef = this.modalService.open(ConfirmDialogComponent)
    modalRef.componentInstance.title = "Delete Messages"
    modalRef.componentInstance.message = `Are you sure you want to delete ${this.selectedMessages.length} selected messages?`
    modalRef.componentInstance.confirmText = "Delete"
    modalRef.componentInstance.confirmButtonClass = "btn-danger"

    modalRef.result
      .then((result) => {
        if (result) {
          this.messageService.bulkDeleteMessages({ ids: this.selectedMessages }).subscribe({
            next: () => {
              this.showSuccess("Messages deleted successfully")
              this.selectedMessages = []
              this.loadMessages()
            },
            error: (error) => {
              console.error("Error deleting messages:", error)
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
