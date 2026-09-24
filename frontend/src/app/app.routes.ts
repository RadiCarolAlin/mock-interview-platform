import { LoginComponent } from './features/auth/pages/login/login.component';
import { Routes } from '@angular/router';

import { MainLayoutComponent } from './layout/main-layout/main-layout.component';

import { homeGuard } from './core/guards/home.guard';
import { interviewerGuard } from './core/guards/interviewer.guard';
import { candidateGuard } from './core/guards/candidate.guard';

// INTERVIEWER - DASHBOARD
import { InterviewerDashboardComponent } from './features/interviewer/dashboard/pages/interviewer-dashboard/interviewer-dashboard.component';

// INTERVIEWER - INTERVIEWS
import { InterviewListComponent } from './features/interviewer/interviews/pages/interview-list/interview-list.component';
import { InterviewFormComponent } from './features/interviewer/interviews/pages/interview-form/interview-form.component';
import { InterviewDetailsComponent } from './features/interviewer/interviews/pages/interview-details/interview-details.component';

// INTERVIEWER - CANDIDATES
import { CandidateListComponent } from './features/interviewer/candidates/pages/candidate-list/candidate-list.component';
import { CandidateFormComponent } from './features/interviewer/candidates/pages/candidate-form/candidate-form.component';
import { CandidateDetailsComponent } from './features/interviewer/candidates/pages/candidate-details/candidate-details.component';

// INTERVIEWER - FEEDBACK
import { FeedbackFormComponent } from './features/interviewer/feedback/pages/feedback-form/feedback-form.component';

// INTERVIEWER - REPORTS
import { ReportsDashboardComponent } from './features/interviewer/reports/pages/reports-dashboard/reports-dashboard.component';

// CANDIDATE PORTAL
import { CandidateDashboardComponent } from './features/candidate/dashboard/pages/candidate-dashboard/candidate-dashboard.component';
import { MyInterviewsComponent } from './features/candidate/interviews/pages/my-interviews/my-interviews.component';
import { MyInterviewDetailsComponent } from './features/candidate/interviews/pages/my-interview-details/my-interview-details.component';
import { MyProgressComponent } from './features/candidate/progress/pages/my-progress/my-progress.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: MainLayoutComponent,

    children: [

      // =========================
      // HOME
      // =========================

      {
        path: '',
        canActivate: [homeGuard],
        children: []
      },

      // =========================
      // INTERVIEWER
      // =========================

      {
        path: 'dashboard',
        component: InterviewerDashboardComponent,
        canActivate: [interviewerGuard]
      },

      {
        path: 'interviews',
        component: InterviewListComponent,
        canActivate: [interviewerGuard]
      },

      {
        path: 'interviews/new',
        component: InterviewFormComponent,
        canActivate: [interviewerGuard]
      },

      {
        path: 'interviews/:id/feedback',
        component: FeedbackFormComponent,
        canActivate: [interviewerGuard]
      },

      {
        path: 'interviews/:id/edit',
        component: InterviewFormComponent,
        canActivate: [interviewerGuard]
      },

      {
        path: 'interviews/:id',
        component: InterviewDetailsComponent,
        canActivate: [interviewerGuard]
      },

      {
        path: 'candidates',
        component: CandidateListComponent,
        canActivate: [interviewerGuard]
      },

      {
        path: 'candidates/new',
        component: CandidateFormComponent,
        canActivate: [interviewerGuard]
      },

      {
        path: 'candidates/:id/edit',
        component: CandidateFormComponent,
        canActivate: [interviewerGuard]
      },

      {
        path: 'candidates/:id',
        component: CandidateDetailsComponent,
        canActivate: [interviewerGuard]
      },

      {
        path: 'reports',
        component: ReportsDashboardComponent,
        canActivate: [interviewerGuard]
      },

      // =========================
      // CANDIDATE
      // =========================

      {
        path: 'my-dashboard',
        component: CandidateDashboardComponent,
        canActivate: [candidateGuard]
      },

      {
        path: 'my-interviews',
        component: MyInterviewsComponent,
        canActivate: [candidateGuard]
      },

      {
        path: 'my-interviews/:id',
        component: MyInterviewDetailsComponent,
        canActivate: [candidateGuard]
      },

      {
        path: 'my-progress',
        component: MyProgressComponent,
        canActivate: [candidateGuard]
      }
    ]
  },

  {
    path: '**',
    redirectTo: ''
  }
];

