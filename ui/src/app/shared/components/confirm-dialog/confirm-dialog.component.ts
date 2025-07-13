import { Component, Input } from "@angular/core"
import { NgbActiveModal, NgbModule } from "@ng-bootstrap/ng-bootstrap" // Import NgbModule
import { CommonModule } from "@angular/common" // Import CommonModule

@Component({
  selector: "app-confirm-dialog",
  standalone: true, // Đánh dấu là standalone component
  imports: [CommonModule, NgbModule], // Import các module cần thiết
  templateUrl: "./confirm-dialog.component.html",
})
export class ConfirmDialogComponent {
  @Input() title = "Confirm"
  @Input() message = "Are you sure?"
  @Input() confirmText = "Confirm"
  @Input() cancelText = "Cancel"
  @Input() confirmButtonClass = "btn-primary"

  constructor(public activeModal: NgbActiveModal) {}

  onConfirm(): void {
    this.activeModal.close(true)
  }

  onCancel(): void {
    this.activeModal.dismiss(false)
  }
}
