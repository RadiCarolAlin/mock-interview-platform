import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Feedback {
  id: string;
  interviewId: string;
  overallScore: number;
  strengths: string;
  improvementAreas: string;
  outcome: number;
  additionalComments: string | null;
}

export interface CreateFeedbackRequest {
  interviewId: string;
  overallScore: number;
  strengths: string;
  improvementAreas: string;
  outcome: number;
  additionalComments: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class FeedbackService {

  private readonly apiUrl =
    'http://localhost:5207/api/Feedback';

  constructor(
    private readonly http: HttpClient
  ) {}

  getByInterviewId(
    interviewId: string
  ): Observable<Feedback> {
    return this.http.get<Feedback>(
      `${this.apiUrl}/interview/${interviewId}`,
      {
        withCredentials: true
      }
    );
  }

  createFeedback(
    request: CreateFeedbackRequest
  ): Observable<Feedback> {
    return this.http.post<Feedback>(
      this.apiUrl,
      request,
      {
        withCredentials: true
      }
    );
  }
}
