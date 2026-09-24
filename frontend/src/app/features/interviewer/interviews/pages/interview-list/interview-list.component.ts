import { apiErrorMessage } from '../../../../../core/utils/api-error';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import {
  Interview
} from '../../models/interview.model';

import {
  InterviewService
} from '../../services/interview.service';

@Component({
  selector: 'app-interview-list',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './interview-list.component.html',
  styleUrl: './interview-list.component.scss'
})
export class InterviewListComponent implements OnInit {

  searchTerm = '';
  selectedStatus = 0;

  interviews: Interview[] = [];

  loading = true;
  error = '';

  constructor(
    private readonly router: Router,
    private readonly interviewService: InterviewService
  ) {}

  ngOnInit(): void {
    this.loadInterviews();
  }

  loadInterviews(): void {

    this.loading = true;
    this.error = '';

    this.interviewService
      .getInterviews()
      .subscribe({

        next: interviews => {

          this.interviews = interviews;

          this.loading = false;
        },

        error: error => {

          console.error(
            'Failed to load interviews',
            error
          );

          this.loading = false;
          this.error = apiErrorMessage(error);
        }

      });
  }

  get filteredInterviews(): Interview[] {

    return this.interviews.filter(interview => {

      const search =
        this.searchTerm
          .trim()
          .toLowerCase();

      const matchesSearch =
        !search ||
        interview.candidateName
          .toLowerCase()
          .includes(search) ||
        interview.title
          .toLowerCase()
          .includes(search);

      const matchesStatus =
        this.selectedStatus === 0 ||
        interview.status === this.selectedStatus;

      return matchesSearch && matchesStatus;
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

  formatDate(value: string): string {

    return new Date(value)
      .toLocaleDateString();
  }

  createInterview(): void {

    void this.router.navigate([
      '/interviews/new'
    ]);
  }

  viewInterview(id: string): void {

    void this.router.navigate([
      '/interviews',
      id
    ]);
  }

  editInterview(id: string): void {

    void this.router.navigate([
      '/interviews',
      id,
      'edit'
    ]);
  }
}

