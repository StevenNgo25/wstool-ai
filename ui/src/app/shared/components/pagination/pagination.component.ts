import { Component, Input, Output, EventEmitter } from "@angular/core"
import { CommonModule } from "@angular/common" // Import CommonModule
import { FormsModule } from "@angular/forms" // Import FormsModule
import { PaginationMetadata } from "../../../models/common.model"

@Component({
  selector: "app-pagination",
  standalone: true, // Đánh dấu là standalone component
  imports: [CommonModule, FormsModule], // Import các module cần thiết
  templateUrl: "./pagination.component.html",
})
export class PaginationComponent {
  @Input() pagination!: PaginationMetadata
  @Output() pageChange = new EventEmitter<number>()
  @Output() pageSizeChange = new EventEmitter<number>()

  onPageChange(page: number, event: Event): void {
    event.preventDefault()
    if (page >= 1 && page <= this.pagination.totalPages && page !== this.pagination.currentPage) {
      this.pageChange.emit(page)
    }
  }

  onPageSizeChange(event: Event): void {
    const target = event.target as HTMLSelectElement
    this.pageSizeChange.emit(Number.parseInt(target.value))
  }

  getVisiblePages(): number[] {
    const current = this.pagination.currentPage
    const total = this.pagination.totalPages
    const delta = 2

    let start = Math.max(1, current - delta)
    let end = Math.min(total, current + delta)

    if (end - start < 2 * delta) {
      if (start === 1) {
        end = Math.min(total, start + 2 * delta)
      } else {
        start = Math.max(1, end - 2 * delta)
      }
    }

    const pages = []
    for (let i = start; i <= end; i++) {
      pages.push(i)
    }
    return pages
  }

  getStartItem(): number {
    return (this.pagination.currentPage - 1) * this.pagination.pageSize + 1
  }

  getEndItem(): number {
    return Math.min(this.pagination.currentPage * this.pagination.pageSize, this.pagination.totalCount)
  }
}
