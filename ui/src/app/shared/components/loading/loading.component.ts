import { Component, Input } from "@angular/core"
import { CommonModule } from "@angular/common" // Import CommonModule

@Component({
  selector: "app-loading",
  standalone: true, // Đánh dấu là standalone component
  imports: [CommonModule], // Import các module cần thiết
  templateUrl: "./loading.component.html",
})
export class LoadingComponent {
  @Input() height = "200px"
  @Input() message = "Loading..."
}
