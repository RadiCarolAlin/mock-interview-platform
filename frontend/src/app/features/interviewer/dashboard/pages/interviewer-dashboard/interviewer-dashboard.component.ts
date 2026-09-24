import { throwError } from 'rxjs';
import { apiErrorMessage } from '../../../../../core/utils/api-error';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import {
  InterviewService
} from '../../../interviews/services/interview.service';

import {
  Interview
} from '../../../interviews/models/interview.model';

import {
  CandidateService
} from '../../../candidates/services/candidate.service';

import {
  Feedback,
  FeedbackService
} from '../../../feedback/services/feedback.service';

interface DashboardInterview {
  id: string;
  candidateName: string;
  title: string;
  time: string;
  status: number;
}

interface RecentInterview {
  id: string;
  candidateName: string;
  title: string;
  date: string;
  score: number | null;
  outcome: number | null;
}

@Component({
  selector: 'app-interviewer-dashboard',
  standalone: true,
  imports: [],
  templateUrl: './interviewer-dashboard.component.html',
  styleUrl: './interviewer-dashboard.component.scss'
})
export class InterviewerDashboardComponent implements OnInit {

  stats = {
    todayInterviews: 0,
    tomorrowInterviews: 0,
    completedThisWeek: 0,
    candidates: 0
  };

  todayInterviews: DashboardInterview[] = [];
  tomorrowInterviews: DashboardInterview[] = [];
  recentInterviews: RecentInterview[] = [];

  loading = true;
  error = '';

  constructor(
    private readonly router: Router,
    private readonly interviewService: InterviewService,
    private readonly candidateService: CandidateService,
    private readonly feedbackService: FeedbackService
  ) {}

  ngOnInit(): void {
    this.loadDashboard();
  }

  private loadDashboard(): void {
    this.loading = true;
    this.error = '';

    forkJoin({
      interviews: this.interviewService.getInterviews(),
      candidates: this.candidateService.getCandidates()
    }).subscribe({
      next: result => {
        this.stats.candidates = result.candidates.length;

        this.buildInterviewData(result.interviews);
        this.loadRecentActivity(result.interviews);
      },

      error: error => {
        console.error(
          'Failed to load dashboard',
          error
        );

        this.loading = false;
        this.error = apiErrorMessage(error);
      }
    });
  }

  private buildInterviewData(
    interviews: Interview[]
  ): void {

    const now = new Date();

    const todayStart = new Date(
      now.getFullYear(),
      now.getMonth(),
      now.getDate()
    );

    const tomorrowStart = new Date(todayStart);
    tomorrowStart.setDate(
      tomorrowStart.getDate() + 1
    );

    const dayAfterTomorrow = new Date(todayStart);
    dayAfterTomorrow.setDate(
      dayAfterTomorrow.getDate() + 2
    );

    const weekStart = new Date(todayStart);

    const day = weekStart.getDay();

    const differenceToMonday =
      day === 0
        ? -6
        : 1 - day;

    weekStart.setDate(
      weekStart.getDate() + differenceToMonday
    );

    const nextWeekStart = new Date(weekStart);
    nextWeekStart.setDate(
      nextWeekStart.getDate() + 7
    );

    const today = interviews.filter(interview => {
      const date = new Date(interview.scheduledAt);

      return (
        date >= todayStart &&
        date < tomorrowStart &&
        interview.status !== 3 &&
        interview.status !== 4
      );
    });

    const tomorrow = interviews.filter(interview => {
      const date = new Date(interview.scheduledAt);

      return (
        date >= tomorrowStart &&
        date < dayAfterTomorrow &&
        interview.status !== 3 &&
        interview.status !== 4
      );
    });

    const completedThisWeek =
      interviews.filter(interview => {
        const date =
          new Date(interview.scheduledAt);

        return (
          interview.status === 3 &&
          date >= weekStart &&
          date < nextWeekStart
        );
      }).length;

    this.todayInterviews =
      today
        .sort(
          (a, b) =>
            new Date(a.scheduledAt).getTime() -
            new Date(b.scheduledAt).getTime()
        )
        .map(interview =>
          this.toDashboardInterview(interview)
        );

    this.tomorrowInterviews =
      tomorrow
        .sort(
          (a, b) =>
            new Date(a.scheduledAt).getTime() -
            new Date(b.scheduledAt).getTime()
        )
        .map(interview =>
          this.toDashboardInterview(interview)
        );

    this.stats.todayInterviews =
      this.todayInterviews.length;

    this.stats.tomorrowInterviews =
      this.tomorrowInterviews.length;

    this.stats.completedThisWeek =
      completedThisWeek;
  }

  private loadRecentActivity(
    interviews: Interview[]
  ): void {

    const completed =
      interviews
        .filter(interview =>
          interview.status === 3
        )
        .sort(
          (a, b) =>
            new Date(b.scheduledAt).getTime() -
            new Date(a.scheduledAt).getTime()
        )
        .slice(0, 5);

    if (completed.length === 0) {
      this.recentInterviews = [];
      this.loading = false;
      return;
    }

    const requests = completed.map(interview =>
      this.feedbackService
        .getByInterviewId(interview.id)
        .pipe(
          catchError(error => error.status === 404 ? of(null) : throwError(() => error))
        )
    );

    forkJoin(requests)
      .subscribe({
        next: feedbackResults => {

          this.recentInterviews =
            completed.map(
              (interview, index) => {

                const feedback:
                  Feedback | null =
                  feedbackResults[index];

                return {
                  id: interview.id,
                  candidateName:
                  interview.candidateName,
                  title:
                  interview.title,
                  date:
                  interview.scheduledAt,
                  score:
                    feedback?.overallScore ?? null,
                  outcome:
                    feedback?.outcome ?? null
                };
              }
            );

          this.loading = false;
        },

        error: error => {
          console.error(
            'Failed to load recent activity',
            error
          );

          this.error = apiErrorMessage(error);
          this.loading = false;
        }
      });
  }

  private toDashboardInterview(
    interview: Interview
  ): DashboardInterview {

    return {
      id: interview.id,
      candidateName:
      interview.candidateName,
      title:
      interview.title,
      time:
        this.getTime(interview.scheduledAt),
      status:
      interview.status
    };
  }

  getTime(value: string): string {
    return new Date(value)
      .toLocaleTimeString([], {
        hour: '2-digit',
        minute: '2-digit'
      });
  }

  getDate(value: string): string {
    return new Date(value)
      .toLocaleDateString();
  }

  getStatusLabel(status: number): string {
    switch (status) {
      case 1:
        return 'Scheduled';

      case 2:
        return 'In Progress';

      case 3:
        return 'Completed';

      case 4:
        return 'Cancelled';

      default:
        return 'Unknown';
    }
  }

  getOutcomeLabel(
    outcome: number | null
  ): string {

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
        return 'No Feedback';
    }
  }

  viewInterview(id: string): void {
    this.router.navigate([
      '/interviews',
      id
    ]);
  }

  createInterview(): void {
    this.router.navigate([
      '/interviews/new'
    ]);
  }
}

