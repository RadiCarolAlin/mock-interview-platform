import { apiErrorMessage } from '../../../../../core/utils/api-error';
import { nonBlank, integer, fieldError } from '../../../../../core/utils/form-validation';
import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { InterviewService } from '../../../interviews/services/interview.service';
import { Interview } from '../../../interviews/models/interview.model';

import {
  CreateFeedbackRequest,
  FeedbackService
} from '../../services/feedback.service';

@Component({
  selector: 'app-feedback-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './feedback-form.component.html',
  styleUrl: './feedback-form.component.scss'
})
export class FeedbackFormComponent implements OnInit {
  readonly fieldError = fieldError;

  interviewId = '';
  interview: Interview | null = null;

  loading = true;
  submitting = false;
  error = '';
  submitError = '';

  outcomes = [
    { value: 1, label: 'Needs More Practice' },
    { value: 2, label: 'Making Progress' },
    { value: 3, label: 'Ready' },
    { value: 4, label: 'Strong Performance' }
  ];

  feedbackForm;

  constructor(
    private readonly fb: FormBuilder,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly interviewService: InterviewService,
    private readonly feedbackService: FeedbackService
  ) {
    this.feedbackForm = this.fb.nonNullable.group({
      overallScore: [
        7,
        [
          Validators.required,
          Validators.min(1),
          Validators.max(10), integer
        ]
      ],

      strengths: [
        '',
        [
          Validators.required, nonBlank,
          Validators.maxLength(2000)
        ]
      ],

      improvementAreas: [
        '',
        [
          Validators.required, nonBlank,
          Validators.maxLength(2000)
        ]
      ],

      outcome: [2, [Validators.required, Validators.pattern(/^[1-4]$/)]],

      additionalComments: [
        '',
        Validators.maxLength(3000)
      ]
    });
  }

  ngOnInit(): void {
    this.interviewId =
      this.route.snapshot.paramMap.get('id') ?? '';

    if (!this.interviewId) {
      this.loading = false;
      this.error = 'Feedback requires an existing completed interview.';
      return;
    }

    this.loadInterview();
  }

  private loadInterview(): void {
    this.loading = true;
    this.error = '';

    this.interviewService
      .getInterview(this.interviewId)
      .subscribe({
        next: interview => {
          this.interview = interview;
          this.loading = false;

          if (interview.status !== 3) {
            this.error = 'Feedback requires an existing completed interview.';
          }
        },

        error: error => {
          this.loading = false;
          this.error = apiErrorMessage(error);
        }
      });
  }

  save(): void {
    if (
      this.submitting || this.loading || !!this.error || this.feedbackForm.invalid ||
      !this.interview
    ) {
      this.feedbackForm.markAllAsTouched();
      return;
    }

    this.submitting = true;
    this.submitError = '';

    const value =
      this.feedbackForm.getRawValue();

    const request: CreateFeedbackRequest = {
      interviewId: this.interviewId,
      overallScore: value.overallScore,
      strengths: value.strengths,
      improvementAreas: value.improvementAreas,
      outcome: Number(value.outcome),

      additionalComments:
        value.additionalComments.trim()
          ? value.additionalComments
          : null
    };

    this.feedbackService
      .createFeedback(request)
      .subscribe({
        next: () => {
          this.router.navigate([
            '/interviews',
            this.interviewId
          ]);
        },

        error: error => {
          console.error(
            'Failed to create feedback',
            error
          );

          this.submitting = false;
          this.submitError = apiErrorMessage(error);
        }
      });
  }

  cancel(): void {
    this.router.navigate([
      '/interviews',
      this.interviewId
    ]);
  }
}

