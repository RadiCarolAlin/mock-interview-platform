import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CandidateListItem {
  id: string;
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  targetRole: string | null;
  experienceLevel: number;
}

export interface CreateCandidateRequest {
  firstName: string;
  lastName: string;
  email: string;
  targetRole: string | null;
  experienceLevel: number;
}

export interface UpdateCandidateRequest {
  firstName: string;
  lastName: string;
  targetRole: string;
  experienceLevel: number;
}

@Injectable({
  providedIn: 'root'
})
export class CandidateService {

  private readonly apiUrl =
    'http://localhost:5207/api/Candidates';

  constructor(
    private readonly http: HttpClient
  ) {}

  getCandidates(
    search?: string
  ): Observable<CandidateListItem[]> {

    let params = new HttpParams();

    if (search?.trim()) {
      params = params.set(
        'search',
        search.trim()
      );
    }

    return this.http.get<CandidateListItem[]>(
      this.apiUrl,
      {
        params,
        withCredentials: true
      }
    );
  }

  getCandidate(
    id: string
  ): Observable<CandidateListItem> {

    return this.http.get<CandidateListItem>(
      `${this.apiUrl}/${id}`,
      {
        withCredentials: true
      }
    );
  }

  createCandidate(
    request: CreateCandidateRequest
  ): Observable<CandidateListItem> {

    return this.http.post<CandidateListItem>(
      this.apiUrl,
      request,
      {
        withCredentials: true
      }
    );
  }

  updateCandidate(
    id: string,
    request: UpdateCandidateRequest
  ): Observable<void> {

    return this.http.put<void>(
      `${this.apiUrl}/${id}`,
      request,
      {
        withCredentials: true
      }
    );
  }
}
