export interface Interview {
  id: string;
  candidateId: string;
  candidateName: string;
  interviewerId: string;
  interviewerName: string;
  title: string;
  type: number;
  level: number;
  scheduledAt: string;
  durationMinutes: number;
  topics: string | null;
  notes: string | null;
  status: number;
}

