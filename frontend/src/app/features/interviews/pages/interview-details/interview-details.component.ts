import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import {
  InterviewService
} from '../../services/interview.service';

import {
  Interview
} from '../../models/interview.model';

import {
  Feedback,
  FeedbackService
} from '../../../feedback/services/feedback.service';

@Component({
  selector: 'app-interview-details',
  standalone: true,
  imports: [],
  templateUrl: './interview-details.component.html',
  styleUrl: './interview-details.component.scss'
})
export class InterviewDetailsComponent implements OnInit {

  interviewId = '';

  interview: Interview | null = null;
  feedback: Feedback | null = null;

  loading = true;
  feedbackLoading = false;
  error = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly interviewService: InterviewService,
    private readonly feedbackService: FeedbackService
  ) {}

  ngOnInit(): void {
    this.interviewId =
      this.route.snapshot.paramMap.get('id') ?? '';

    if (!this.interviewId) {
      this.loading = false;
      this.error = true;
      return;
    }

    this.loadInterview();
  }

  private loadInterview(): void {
    this.loading = true;
    this.error = false;

    this.interviewService
      .getInterview(this.interviewId)
      .subscribe({
        next: interview => {
          this.interview = interview;
          this.loading = false;

          if (interview.status === 3) {
            this.loadFeedback();
          } else {
            this.feedback = null;
          }
        },

        error: () => {
          this.loading = false;
          this.error = true;
        }
      });
  }

  private loadFeedback(): void {
    this.feedbackLoading = true;

    this.feedbackService
      .getByInterviewId(this.interviewId)
      .subscribe({
        next: feedback => {
          this.feedback = feedback;
          this.feedbackLoading = false;
        },

        error: error => {
          this.feedbackLoading = false;

          if (error.status === 404) {
            this.feedback = null;
            return;
          }

          console.error(
            'Failed to load feedback',
            error
          );
        }
      });
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

  getTypeLabel(type: number): string {
    switch (type) {
      case 1:
        return 'Technical';

      case 2:
        return 'System Design';

      case 3:
        return 'Behavioral';

      default:
        return 'Unknown';
    }
  }

  getLevelLabel(level: number): string {
    switch (level) {
      case 1:
        return 'Junior';

      case 2:
        return 'Mid';

      case 3:
        return 'Senior';

      case 4:
        return 'Lead';

      default:
        return 'Unknown';
    }
  }

  getOutcomeLabel(outcome: number): string {
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
        return 'Unknown';
    }
  }

  getDate(value: string): string {
    return new Date(value)
      .toLocaleDateString();
  }

  getTime(value: string): string {
    return new Date(value)
      .toLocaleTimeString([], {
        hour: '2-digit',
        minute: '2-digit'
      });
  }

  getTopics(topics: string | null): string[] {
    if (!topics) {
      return [];
    }

    return topics
      .split(',')
      .map(topic => topic.trim())
      .filter(topic => topic.length > 0);
  }

  back(): void {
    this.router.navigate(['/interviews']);
  }

  editInterview(): void {
    this.router.navigate([
      '/interviews',
      this.interviewId,
      'edit'
    ]);
  }

  viewCandidate(): void {
    if (!this.interview) {
      return;
    }

    this.router.navigate([
      '/candidates',
      this.interview.candidateId
    ]);
  }

  completeInterview(): void {
    if (!this.interview) {
      return;
    }

    this.interviewService
      .completeInterview(this.interviewId)
      .subscribe({
        next: () => {
          this.loadInterview();
        },

        error: () => {
          this.error = true;
        }
      });
  }

  addFeedback(): void {
    this.router.navigate([
      '/interviews',
      this.interviewId,
      'feedback'
    ]);
  }
}
