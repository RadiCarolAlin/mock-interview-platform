export type UserRole = 'Interviewer' | 'Candidate';

export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: UserRole;

  candidateId: string | null;
  interviewerId: string | null;
}
