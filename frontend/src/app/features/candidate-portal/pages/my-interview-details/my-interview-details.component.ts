import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import {
  CandidateInterviewDetails,
  CandidatePortalService
} from '../../services/candidate-portal.service';

@Component({
  selector: 'app-my-interview-details',
  standalone: true,
  imports: [],
  templateUrl: './my-interview-details.component.html',
  styleUrl: './my-interview-details.component.scss'
})
export class MyInterviewDetailsComponent implements OnInit {

  interview: CandidateInterviewDetails | null = null;

  loading = true;
  notFound = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly candidatePortalService: CandidatePortalService
  ) {}

  ngOnInit(): void {
    const interviewId =
      this.route.snapshot.paramMap.get('id');

    if (!interviewId) {
      this.loading = false;
      this.notFound = true;
      return;
    }

    this.candidatePortalService
      .getInterview(interviewId)
      .subscribe({
        next: interview => {
          this.interview = interview;
          this.loading = false;
        },

        error: error => {
          console.error(
            'Failed to load interview details',
            error
          );

          this.loading = false;
          this.notFound = true;
        }
      });
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

  getTopics(): string[] {
    if (!this.interview?.topics) {
      return [];
    }

    return this.interview.topics
      .split(',')
      .map(topic => topic.trim())
      .filter(topic => topic.length > 0);
  }

  back(): void {
    void this.router.navigate(['/my-interviews']);
  }
}

