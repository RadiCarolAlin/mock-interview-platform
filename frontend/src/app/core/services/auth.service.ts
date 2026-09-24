import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { apiErrorMessage } from '../utils/api-error';
import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

import { User, UserRole } from '../models/user.model';

interface AuthMeResponse {
  isAuthenticated: boolean;
  userId: string;
  oktaUserId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  candidateId: string | null;
  interviewerId: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private readonly apiUrl = '/api/auth';
  private readonly router = inject(Router);

  accessError = signal('');
  private loginStarted = false;

  currentUser = signal<User | null>(null);

  constructor(private readonly http: HttpClient) {
  }

  loadCurrentUser(): Observable<AuthMeResponse> {
    return this.http
      .get<AuthMeResponse>(
        `${this.apiUrl}/me`,
        {
          withCredentials: true
        }
      )
      .pipe(
        tap(response => {
          this.accessError.set('');
          this.currentUser.set({
            id: response.userId,
            firstName: response.firstName,
            lastName: response.lastName,
            email: response.email,
            role: response.role,
            candidateId: response.candidateId,
            interviewerId: response.interviewerId
          });
        })
      );
  }

  handleAccessError(error: unknown): void {
    if (error instanceof HttpErrorResponse && error.status === 401) {
      this.currentUser.set(null);
      this.accessError.set('');
      void this.router.navigate(['/login'], { replaceUrl: true });
    }
    else this.accessError.set(apiErrorMessage(error));
  }

  login(): void {
    if (this.loginStarted) return;
    this.loginStarted = true;
    window.location.href = `${this.apiUrl}/login`;
  }

  logout(): void {
    this.currentUser.set(null);
    window.location.href = `${this.apiUrl}/logout`;
  }

  isAuthenticated(): boolean {
    return this.currentUser() !== null;
  }

  isInterviewer(): boolean {
    return this.currentUser()?.role === 'Interviewer';
  }

  isCandidate(): boolean {
    return this.currentUser()?.role === 'Candidate';
  }
}

