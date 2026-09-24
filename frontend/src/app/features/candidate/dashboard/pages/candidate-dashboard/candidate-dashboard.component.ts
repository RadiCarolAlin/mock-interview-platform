import { apiErrorMessage } from '../../../../../core/utils/api-error';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';

import {
  CandidateInterview,
  CandidatePortalService,
  CandidateProgressInterview
} from '../../../services/candidate-portal.service';

@Component({
  selector: 'app-candidate-dashboard',
  standalone: true,
  imports: [],
  templateUrl: './candidate-dashboard.component.html',
  styleUrl: './candidate-dashboard.component.scss'
})
export class CandidateDashboardComponent implements OnInit {

  loading = true;
  error = '';

  stats = {
    completedInterviews: 0,
    upcomingInterviews: 0,
    averageScore: 0,
    latestScore: 0
  };

  upcomingInterview: CandidateInterview | null = null;

  latestInterview: CandidateProgressInterview | null = null;

  constructor(
    private readonly router: Router,
    private readonly candidatePortalService: CandidatePortalService
  ) {}

  ngOnInit(): void {
    forkJoin({
      interviews: this.candidatePortalService.getInterviews(),
      progress: this.candidatePortalService.getProgress()
    }).subscribe({
      next: ({ interviews, progress }) => {

        const now = new Date();

        const upcomingInterviews = interviews
          .filter(interview =>
            interview.status === 1 &&
            new Date(interview.scheduledAt) > now
          )
          .sort(
            (a, b) =>
              new Date(a.scheduledAt).getTime() -
              new Date(b.scheduledAt).getTime()
          );

        const scoredInterviews = progress.interviews
          .filter(interview => interview.score !== null)
          .sort(
            (a, b) =>
              new Date(b.scheduledAt).getTime() -
              new Date(a.scheduledAt).getTime()
          );

        this.upcomingInterview =
          upcomingInterviews[0] ?? null;

        this.latestInterview =
          scoredInterviews[0] ?? null;

        this.stats = {
          completedInterviews:
          progress.completedInterviews,

          upcomingInterviews:
          upcomingInterviews.length,

          averageScore:
            progress.averageScore ?? 0,

          latestScore:
            this.latestInterview?.score ?? 0
        };

        this.loading = false;
      },

      error: error => {
        console.error(
          'Failed to load candidate dashboard',
          error
        );

        this.loading = false;
        this.error = apiErrorMessage(error);
      }
    });
  }

  getOutcomeLabel(outcome: number | null): string {
    switch (outcome) {
      case 1:
        return 'Needs More Practice';

      case 2:
        return 'Making Progress';

      case 3:
        return 'Ready';

      case 4:
        return 'Strong Performance';

      default:
        return 'No outcome';
    }
  }

  formatDate(value: string): string {
    return new Date(value).toLocaleDateString();
  }

  formatTime(value: string): string {
    return new Date(value).toLocaleTimeString(
      [],
      {
        hour: '2-digit',
        minute: '2-digit'
      }
    );
  }

  viewProgress(): void {
    void this.router.navigate(['/my-progress']);
  }
}

