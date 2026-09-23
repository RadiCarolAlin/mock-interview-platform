import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Interview } from '../models/interview.model';

export interface CreateInterviewRequest {
  candidateId: string;
  title: string;
  type: number;
  level: number;
  scheduledAt: string;
  durationMinutes: number;
  topics: string | null;
  notes: string | null;
}

export interface UpdateInterviewRequest {
  title: string;
  type: number;
  level: number;
  scheduledAt: string;
  durationMinutes: number;
  topics: string | null;
  notes: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class InterviewService {

  private readonly apiUrl = 'http://localhost:5207/api/Interviews';

  constructor(private readonly http: HttpClient) {}

  getInterviews(): Observable<Interview[]> {
    return this.http.get<Interview[]>(
      this.apiUrl,
      { withCredentials: true }
    );
  }

  getInterview(id: string): Observable<Interview> {
    return this.http.get<Interview>(
      `${this.apiUrl}/${id}`,
      { withCredentials: true }
    );
  }

  createInterview(
    request: CreateInterviewRequest
  ): Observable<Interview> {
    return this.http.post<Interview>(
      this.apiUrl,
      request,
      { withCredentials: true }
    );
  }

  updateInterview(
    id: string,
    request: UpdateInterviewRequest
  ): Observable<void> {
    return this.http.put<void>(
      `${this.apiUrl}/${id}`,
      request,
      { withCredentials: true }
    );
  }

  completeInterview(id: string): Observable<void> {
    return this.http.patch<void>(
      `${this.apiUrl}/${id}/complete`,
      {},
      { withCredentials: true }
    );
  }
}
