export type InterviewOutcome =
  | 'Needs More Practice'
  | 'Making Progress'
  | 'Ready'
  | 'Strong Performance';

export interface Feedback {
  id: number;
  interviewId: number;

  overallScore: number;

  strengths: string;
  improvementAreas: string;

  outcome: InterviewOutcome;

  additionalComments?: string;
}

