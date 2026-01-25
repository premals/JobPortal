import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container" *ngIf="message">
      <div class="toast-success">
        {{ message }}
      </div>
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 9999;
    }
    .toast-success {
      background: #198754;
      color: white;
      padding: 12px 16px;
      border-radius: 4px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.2);
    }
  `]
})
export class ToastComponent {
  message = '';

  constructor(private toastService: ToastService) {
    this.toastService.toast$.subscribe(msg => {
      this.message = msg;
      setTimeout(() => this.message = '', 3000);
    });
  }
}
