import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ReportOverview {
  totalCandidates: number;
  totalInterviews: number;
  completedInterviews: number;
  scheduledInterviews: number;
  averageScore: number | null;
}

export interface CandidateProgressInterview {
  interviewId: string;
  title: string;
  scheduledAt: string;
  score: number | null;
  outcome: number | null;
}

export interface CandidateProgress {
  candidateId: string;
  candidateName: string;
  totalInterviews: number;
  completedInterviews: number;
  averageScore: number | null;
  latestOutcome: number | null;
  interviews: CandidateProgressInterview[];
}

@Injectable({
  providedIn: 'root'
})
export class ReportService {

  private readonly apiUrl =
    '/api/Reports';

  constructor(
    private readonly http: HttpClient
  ) {}

  getOverview(): Observable<ReportOverview> {
    return this.http.get<ReportOverview>(
      `${this.apiUrl}/overview`,
      {
        withCredentials: true
      }
    );
  }

  getCandidateProgress(
    candidateId: string
  ): Observable<CandidateProgress> {
    return this.http.get<CandidateProgress>(
      `${this.apiUrl}/candidates/${candidateId}/progress`,
      {
        withCredentials: true
      }
    );
  }
}

