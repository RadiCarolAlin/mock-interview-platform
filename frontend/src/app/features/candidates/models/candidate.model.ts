
export type ExperienceLevel =
  'Junior' |
  'Mid' |
  'Senior' |
  'Lead';

export interface Candidate {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  targetRole?: string;
  experienceLevel?: ExperienceLevel;
}
