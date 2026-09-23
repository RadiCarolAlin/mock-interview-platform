import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CandidateInterview {
  id: string;
  candidateId: string;
  candidateName: string;
  title: string;
  type: number;
  level: number;
  scheduledAt: string;
  durationMinutes: number;
  status: number;
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

export interface CandidateInterviewFeedback {
  overallScore: number;
  outcome: number;
  strengths: string;
  improvementAreas: string;
  additionalComments: string | null;
}

export interface CandidateInterviewDetails {
  id: string;
  title: string;
  interviewer: string;
  type: number;
  level: number;
  scheduledAt: string;
  durationMinutes: number;
  topics: string | null;
  notes: string | null;
  status: number;
  feedback: CandidateInterviewFeedback | null;
}
@Injectable({
  providedIn: 'root'
})
export class CandidatePortalService {

  private readonly apiUrl =
    'http://localhost:5207/api/candidate/me';

  constructor(
    private readonly http: HttpClient
  ) {}

  getInterviews(): Observable<CandidateInterview[]> {
    return this.http.get<CandidateInterview[]>(
      `${this.apiUrl}/interviews`,
      {
        withCredentials: true
      }
    );
  }

  getProgress(): Observable<CandidateProgress> {
    return this.http.get<CandidateProgress>(
      `${this.apiUrl}/progress`,
      {
        withCredentials: true
      }
    );
  }

  getInterview(
    interviewId: string
  ): Observable<CandidateInterviewDetails> {
    return this.http.get<CandidateInterviewDetails>(
      `${this.apiUrl}/interviews/${interviewId}`,
      {
        withCredentials: true
      }
    );
  }
}
