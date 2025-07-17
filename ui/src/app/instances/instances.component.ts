import { Component, OnInit, TemplateRef, ViewChild } from "@angular/core"
import { CommonModule } from "@angular/common" // Import CommonModule
import { FormsModule } from "@angular/forms" // Import FormsModule
import { RouterModule } from "@angular/router" // Import RouterModule
import { NgbModal, NgbModalModule } from "@ng-bootstrap/ng-bootstrap" // Import NgbModalModule
import { ToastaService, ToastOptions, ToastaModule } from "ngx-toasta" // Import ToastaModule

import { Router } from "@angular/router"
import { InstanceService } from "../services/instance.service"
import { Instance } from "../models/user.model"
import { PaginationRequest, PaginatedResponse } from "../models/common.model"
import { ConfirmDialogComponent } from "../shared/components/confirm-dialog/confirm-dialog.component"
import { PaginationComponent } from "../shared/components/pagination/pagination.component"
import { LoadingComponent } from "../shared/components/loading/loading.component"
import { AuthService } from "../services/auth.service"

@Component({
  selector: "app-instances",
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
  templateUrl: "./instances.component.html",
  styleUrls: ["./instances.component.scss"],
})
export class InstancesComponent implements OnInit {
  @ViewChild('qrModal') qrModal!: TemplateRef<any>;
  @ViewChild('codeModal') codeModal!: TemplateRef<any>;
  instances: PaginatedResponse<Instance> = { data: [], pagination: {} as any }
  loading = true
  searchTerm = ""
  getCode_instanceId:number|null = null
  phoneNumber = null
  showCode = false

  paginationRequest: PaginationRequest = {
    page: 1,
    pageSize: 10,
    search: "",
    sortBy: "createdAt",
    sortDirection: "desc",
  }

  isAdmin = false
  qrCode: string | null = null;
  code: string | null = null;

  constructor(
    private instanceService: InstanceService,
    public router: Router,
    private modalService: NgbModal,
    private toastaService: ToastaService,
    private authService: AuthService,
  ) {
    this.isAdmin = this.authService.isAdmin()
  }

  ngOnInit(): void {
    this.loadInstances()
  }

  loadInstances(): void {
    this.loading = true
    this.instanceService.getInstances(this.paginationRequest).subscribe({
      next: (response) => {
        this.instances = response
        this.loading = false
      },
      error: (error) => {
        console.error("Error loading instances:", error)
        this.loading = false
      },
    })
  }

  search(): void {
    this.paginationRequest.search = this.searchTerm
    this.paginationRequest.page = 1
    this.loadInstances()
  }

  onPageChange(page: number): void {
    this.paginationRequest.page = page
    this.loadInstances()
  }

  onPageSizeChange(pageSize: number): void {
    this.paginationRequest.pageSize = pageSize
    this.paginationRequest.page = 1
    this.loadInstances()
  }

  createInstance(): void {
    this.router.navigate(["/instances/new"])
  }

  editInstance(id: number): void {
    this.router.navigate(["/instances/edit", id])
  }

  deleteInstance(id: number): void {
    const modalRef = this.modalService.open(ConfirmDialogComponent)
    modalRef.componentInstance.title = "Delete Instance"
    modalRef.componentInstance.message = "Are you sure you want to delete this instance? This action cannot be undone."
    modalRef.componentInstance.confirmText = "Delete"
    modalRef.componentInstance.confirmButtonClass = "btn-danger"

    modalRef.result
      .then((result) => {
        if (result) {
          this.instanceService.deleteInstance(id).subscribe({
            next: () => {
              this.showSuccess("Instance deleted successfully!")
              this.loadInstances()
            },
            error: (error) => {
              console.error("Error deleting instance:", error)
            },
          })
        }
      })
      .catch(() => {})
  }
  scanQr(instanceId: number) {
    this.instanceService.getQrCode(instanceId).subscribe((res) => {
      console.log(res)
      this.qrCode = res.qr;
      this.modalService.open(this.qrModal);
    });
  }

  showCodeModal(instanceId: number) {
    this.getCode_instanceId = instanceId
    this.modalService.open(this.codeModal);
  }
  
  getCode() {
    if (this.phoneNumber && this.getCode_instanceId) {
      this.instanceService.getCode(this.getCode_instanceId, this.phoneNumber).subscribe((res) => {
        this.code = res.code;
        this.showCode = true;
      });
    } else {
      alert('Vui lòng nhập số điện thoại');
    }
  }
  
  logoutInstance(instanceId: number) {
    /* if (confirm('Bạn có chắc chắn muốn đăng xuất tài khoản này?')) {
      this.apiService.logoutInstance(instanceId).subscribe(() => {
        this.loadInstances(); // load lại danh sách
      });
    } */
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
