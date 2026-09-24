import { apiErrorMessage } from '../../../../../core/utils/api-error';
import {
  Component,
  OnInit
} from '@angular/core';

import {
  Router
} from '@angular/router';

import {
  ReportOverview,
  ReportService
} from '../../services/report.service';

@Component({
  selector: 'app-reports-dashboard',
  standalone: true,
  imports: [],
  templateUrl: './reports-dashboard.component.html',
  styleUrl: './reports-dashboard.component.scss'
})
export class ReportsDashboardComponent implements OnInit {

  stats: ReportOverview = {
    totalCandidates: 0,
    totalInterviews: 0,
    completedInterviews: 0,
    scheduledInterviews: 0,
    averageScore: null
  };

  loading = true;
  error = '';

  constructor(
    private readonly router: Router,
    private readonly reportService: ReportService
  ) {}

  ngOnInit(): void {
    this.loadOverview();
  }

  private loadOverview(): void {
    this.loading = true;
    this.error = '';

    this.reportService
      .getOverview()
      .subscribe({
        next: overview => {
          this.stats = overview;
          this.loading = false;
        },

        error: error => {
          console.error(
            'Failed to load reports overview',
            error
          );

          this.loading = false;
          this.error = apiErrorMessage(error);
        }
      });
  }

  getAverageScore(): string {
    if (this.stats.averageScore === null) {
      return '-';
    }

    return this.stats.averageScore
      .toFixed(1);
  }

  viewCandidates(): void {
    this.router.navigate(['/candidates']);
  }
}

