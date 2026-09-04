import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { TranslocoModule } from '@jsverse/transloco';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [RouterLink, TranslocoModule],
  templateUrl: './signup.component.html',
  styleUrl: './signup.component.scss'
})
export class SignupComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  email = signal('');
  username = signal('');
  password = signal('');
  error = signal<string | null>(null);
  submitting = signal(false);

  async submit() {
    const email = this.email().trim();
    const username = this.username().trim();
    const password = this.password();
    if (!email || !username || !password) return;

    if (password.length < 8) {
      this.error.set('auth.passwordTooShort');
      return;
    }

    this.error.set(null);
    this.submitting.set(true);
    try {
      await this.auth.signup(email, username, password);
      this.router.navigate(['/']);
    } catch (err) {
      if (err instanceof HttpErrorResponse && err.status === 409) {
        this.error.set('auth.emailTaken');
      } else {
        this.error.set('auth.genericError');
      }
    } finally {
      this.submitting.set(false);
    }
  }
}
