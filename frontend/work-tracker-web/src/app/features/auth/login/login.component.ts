import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { TranslocoModule } from '@jsverse/transloco';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [RouterLink, TranslocoModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  email = signal('');
  password = signal('');
  error = signal<string | null>(null);
  submitting = signal(false);

  async submit() {
    const email = this.email().trim();
    const password = this.password();
    if (!email || !password) return;

    this.error.set(null);
    this.submitting.set(true);
    try {
      await this.auth.login(email, password);
      this.router.navigate(['/']);
    } catch (err) {
      if (err instanceof HttpErrorResponse && err.status === 401) {
        this.error.set('auth.invalidCredentials');
      } else {
        this.error.set('auth.genericError');
      }
    } finally {
      this.submitting.set(false);
    }
  }
}
