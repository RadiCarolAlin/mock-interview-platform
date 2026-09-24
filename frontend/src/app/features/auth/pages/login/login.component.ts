import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { apiErrorMessage } from '../../../../core/utils/api-error';

@Component({
  selector: 'app-login',
  standalone: true,
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  checkingSession = true;
  signingIn = false;
  error = '';

  ngOnInit(): void {
    this.checkSession();
  }

  checkSession(): void {
    this.checkingSession = true;
    this.error = '';
    this.auth.accessError.set('');
    this.auth.loadCurrentUser().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: user => {
        void this.router.navigate(
          [user.role === 'Interviewer' ? '/dashboard' : '/my-dashboard'],
          { replaceUrl: true }
        );
      },
      error: (error: unknown) => {
        this.checkingSession = false;
        if (error instanceof HttpErrorResponse && error.status === 401) {
          this.auth.currentUser.set(null);
        } else {
          this.error = apiErrorMessage(error);
        }
      }
    });
  }

  signIn(): void {
    if (this.checkingSession || this.signingIn) return;
    this.signingIn = true;
    this.auth.login();
  }
}
