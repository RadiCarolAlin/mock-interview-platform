import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';

import {
  CandidateListItem,
  CandidateService
} from '../../services/candidate.service';

import {
  Interview
} from '../../../interviews/models/interview.model';

import {
  InterviewService
} from '../../../interviews/services/interview.service';

@Component({
  selector: 'app-candidate-details',
  standalone: true,
  imports: [],
  templateUrl: './candidate-details.component.html',
  styleUrl: './candidate-details.component.scss'
})
export class CandidateDetailsComponent implements OnInit {

  candidateId = '';

  candidate: CandidateListItem | null = null;

  interviews: Interview[] = [];

  loading = true;
  error = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly candidateService: CandidateService,
    private readonly interviewService: InterviewService
  ) {}

  ngOnInit(): void {

    this.candidateId =
      this.route.snapshot.paramMap.get('id') ?? '';

    if (!this.candidateId) {
      this.loading = false;
      this.error = true;
      return;
    }

    forkJoin({
      candidate:
        this.candidateService.getCandidate(this.candidateId),

      interviews:
        this.interviewService.getInterviews()
    }).subscribe({

      next: ({ candidate, interviews }) => {

        this.candidate = candidate;

        this.interviews = interviews
          .filter(
            interview =>
              interview.candidateId === this.candidateId
          )
          .sort(
            (a, b) =>
              new Date(b.scheduledAt).getTime() -
              new Date(a.scheduledAt).getTime()
          );

        this.loading = false;
      },

      error: error => {

        console.error(
          'Failed to load candidate details',
          error
        );

        this.loading = false;
        this.error = true;
      }

    });
  }

  getExperienceLevel(level: number): string {

    switch (level) {
      case 1: return 'Junior';
      case 2: return 'Mid';
      case 3: return 'Senior';
      case 4: return 'Lead';
      default: return 'Unknown';
    }
  }

  getInterviewType(type: number): string {

    switch (type) {
      case 1: return 'Technical';
      case 2: return 'System Design';
      case 3: return 'Behavioral';
      default: return 'Unknown';
    }
  }

  getInterviewStatus(status: number): string {

    switch (status) {
      case 1: return 'Scheduled';
      case 2: return 'In Progress';
      case 3: return 'Completed';
      case 4: return 'Cancelled';
      default: return 'Unknown';
    }
  }

  formatDate(value: string): string {
    return new Date(value).toLocaleDateString();
  }

  back(): void {
    void this.router.navigate(['/candidates']);
  }

  editCandidate(): void {

    void this.router.navigate([
      '/candidates',
      this.candidateId,
      'edit'
    ]);
  }

  viewInterview(id: string): void {

    void this.router.navigate([
      '/interviews',
      id
    ]);
  }
}
