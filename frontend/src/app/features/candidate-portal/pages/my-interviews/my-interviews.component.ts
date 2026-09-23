import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';

import {
  CandidatePortalService
} from '../../services/candidate-portal.service';

interface MyInterview {
  id: string;
  title: string;
  scheduledAt: string;
  type: string;
  status: string;
  score: number | null;
}

@Component({
  selector: 'app-my-interviews',
  standalone: true,
  imports: [],
  templateUrl: './my-interviews.component.html',
  styleUrl: './my-interviews.component.scss'
})
export class MyInterviewsComponent implements OnInit {

  interviews: MyInterview[] = [];

  loading = true;
  error = false;

  completedCount = 0;
  upcomingCount = 0;

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

        this.interviews = interviews.map(interview => {

          const progressInterview =
            progress.interviews.find(
              item => item.interviewId === interview.id
            );

          return {
            id: interview.id,
            title: interview.title,
            scheduledAt: interview.scheduledAt,
            type: this.getTypeLabel(interview.type),
            status: this.getStatusLabel(interview.status),
            score: progressInterview?.score ?? null
          };
        });

        this.completedCount = this.interviews.filter(
          interview => interview.status === 'Completed'
        ).length;

        this.upcomingCount = this.interviews.filter(
          interview => interview.status === 'Scheduled'
        ).length;

        this.loading = false;
      },

      error: error => {
        console.error(
          'Failed to load candidate interviews',
          error
        );

        this.loading = false;
        this.error = true;
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

  viewInterview(id: string): void {
    void this.router.navigate([
      '/my-interviews',
      id
    ]);
  }
}
