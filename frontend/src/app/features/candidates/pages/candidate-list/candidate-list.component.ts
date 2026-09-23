import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import {
  CandidateListItem,
  CandidateService
} from '../../services/candidate.service';

@Component({
  selector: 'app-candidate-list',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './candidate-list.component.html',
  styleUrl: './candidate-list.component.scss'
})
export class CandidateListComponent implements OnInit {

  searchTerm = '';

  candidates: CandidateListItem[] = [];

  loading = true;
  error = false;

  constructor(
    private readonly router: Router,
    private readonly candidateService: CandidateService
  ) {}

  ngOnInit(): void {
    this.loadCandidates();
  }

  loadCandidates(): void {

    this.loading = true;
    this.error = false;

    this.candidateService
      .getCandidates(this.searchTerm)
      .subscribe({

        next: candidates => {
          this.candidates = candidates;
          this.loading = false;
        },

        error: error => {
          console.error(
            'Failed to load candidates',
            error
          );

          this.loading = false;
          this.error = true;
        }

      });
  }

  search(): void {
    this.loadCandidates();
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.loadCandidates();
  }

  getExperienceLevel(
    level: number
  ): string {

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

  addCandidate(): void {
    void this.router.navigate([
      '/candidates/new'
    ]);
  }

  viewCandidate(id: string): void {
    void this.router.navigate([
      '/candidates',
      id
    ]);
  }

  editCandidate(id: string): void {
    void this.router.navigate([
      '/candidates',
      id,
      'edit'
    ]);
  }
}
