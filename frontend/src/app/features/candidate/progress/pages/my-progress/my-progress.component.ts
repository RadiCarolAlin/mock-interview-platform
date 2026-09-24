import { apiErrorMessage } from '../../../../../core/utils/api-error';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import {
  CandidatePortalService
} from '../../../services/candidate-portal.service';

interface ProgressSession {
  id: string;
  title: string;
  scheduledAt: string;
  score: number;
  outcome: number | null;
}

@Component({
  selector: 'app-my-progress',
  standalone: true,
  imports: [],
  templateUrl: './my-progress.component.html',
  styleUrl: './my-progress.component.scss'
})
export class MyProgressComponent implements OnInit {

  loading = true;
  error = '';

  summary = {
    completedSessions: 0,
    averageScore: 0,
    latestScore: 0,
    improvement: 0
  };

  sessions: ProgressSession[] = [];

  constructor(
    private readonly router: Router,
    private readonly candidatePortalService: CandidatePortalService
  ) {}

  ngOnInit(): void {
    this.candidatePortalService
      .getProgress()
      .subscribe({
        next: progress => {

          this.sessions = progress.interviews
            .filter(
              interview => interview.score !== null
            )
            .map(interview => ({
              id: interview.interviewId,
              title: interview.title,
              scheduledAt: interview.scheduledAt,
              score: interview.score ?? 0,
              outcome: interview.outcome
            }))
            .sort(
              (a, b) =>
                new Date(a.scheduledAt).getTime() -
                new Date(b.scheduledAt).getTime()
            );

          const firstScore =
            this.sessions.length > 0
              ? this.sessions[0].score
              : 0;

          const latestScore =
            this.sessions.length > 0
              ? this.sessions[this.sessions.length - 1].score
              : 0;

          this.summary = {
            completedSessions:
            progress.completedInterviews,

            averageScore:
              progress.averageScore ?? 0,

            latestScore,

            improvement:
              this.sessions.length > 1
                ? latestScore - firstScore
                : 0
          };

          this.loading = false;
        },

        error: error => {
          console.error(
            'Failed to load candidate progress',
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
        return 'No Outcome';
    }
  }

  formatDate(value: string): string {
    return new Date(value).toLocaleDateString();
  }

  viewInterview(id: string): void {
    void this.router.navigate([
      '/my-interviews',
      id
    ]);
  }
}

