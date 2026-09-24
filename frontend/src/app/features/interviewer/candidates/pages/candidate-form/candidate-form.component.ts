import { apiErrorMessage } from '../../../../../core/utils/api-error';
import { nonBlank, fieldError } from '../../../../../core/utils/form-validation';
import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import {
  ActivatedRoute,
  Router,
  RouterLink
} from '@angular/router';

import {
  CandidateService,
  CreateCandidateRequest,
  UpdateCandidateRequest
} from '../../services/candidate.service';

interface ExperienceLevelOption {
  label: string;
  value: number;
}

@Component({
  selector: 'app-candidate-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './candidate-form.component.html',
  styleUrl: './candidate-form.component.scss'
})
export class CandidateFormComponent implements OnInit {
  readonly fieldError = fieldError;
  loadError = '';

  experienceLevels: ExperienceLevelOption[] = [
    { label: 'Junior', value: 1 },
    { label: 'Mid', value: 2 },
    { label: 'Senior', value: 3 },
    { label: 'Lead', value: 4 }
  ];

  candidateId: string | null = null;

  isEditMode = false;
  loading = false;
  submitting = false;

  submitError = '';

  candidateForm;

  constructor(
    private readonly fb: FormBuilder,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly candidateService: CandidateService
  ) {

    this.candidateForm =
      this.fb.nonNullable.group({

        firstName: ['', [
          Validators.required, nonBlank,
          Validators.maxLength(100)
        ]],

        lastName: ['', [
          Validators.required, nonBlank,
          Validators.maxLength(100)
        ]],

        email: ['', [
          Validators.required, nonBlank,
          Validators.email,
          Validators.maxLength(255)
        ]],

        targetRole: ['', [
          Validators.maxLength(150)
        ]],

        experienceLevel: [2, [Validators.required, Validators.pattern(/^[1-4]$/)]]
      });
  }

  ngOnInit(): void {

    this.candidateId =
      this.route.snapshot.paramMap.get('id');

    this.isEditMode =
      !!this.candidateId;

    if (this.isEditMode && this.candidateId) {
      this.loadCandidate(this.candidateId);
    }
  }

  private loadCandidate(id: string): void {

    this.loading = true;
    this.submitError = '';

    this.candidateService
      .getCandidate(id)
      .subscribe({

        next: candidate => {

          this.candidateForm.patchValue({
            firstName: candidate.firstName,
            lastName: candidate.lastName,
            email: candidate.email,
            targetRole: candidate.targetRole ?? '',
            experienceLevel: candidate.experienceLevel
          });

          this.candidateForm.controls.email.disable();

          this.loading = false;
        },

        error: error => {

          console.error(
            'Failed to load candidate',
            error
          );

          this.loading = false;

          this.loadError = apiErrorMessage(error);
        }

      });
  }

  submit(): void {

    if (
      this.loading || !!this.loadError || this.candidateForm.invalid ||
      this.submitting
    ) {
      this.candidateForm.markAllAsTouched();
      return;
    }

    this.submitting = true;
    this.submitError = '';

    const formValue =
      this.candidateForm.getRawValue();

    if (
      this.isEditMode &&
      this.candidateId
    ) {

      const request: UpdateCandidateRequest = {
        firstName:
          formValue.firstName.trim(),

        lastName:
          formValue.lastName.trim(),

        targetRole:
          formValue.targetRole.trim(),

        experienceLevel:
        formValue.experienceLevel
      };

      this.candidateService
        .updateCandidate(
          this.candidateId,
          request
        )
        .subscribe({

          next: () => {

            this.submitting = false;

            void this.router.navigate([
              '/candidates',
              this.candidateId
            ]);
          },

          error: error => {

            console.error(
              'Failed to update candidate',
              error
            );

            this.submitting = false;

            this.submitError = apiErrorMessage(error);
          }

        });

      return;
    }

    const request: CreateCandidateRequest = {
      firstName:
        formValue.firstName.trim(),

      lastName:
        formValue.lastName.trim(),

      email:
        formValue.email.trim(),

      targetRole:
        formValue.targetRole.trim(),

      experienceLevel:
      formValue.experienceLevel
    };

    this.candidateService
      .createCandidate(request)
      .subscribe({

        next: candidate => {

          this.submitting = false;

          void this.router.navigate([
            '/candidates',
            candidate.id
          ]);
        },

        error: error => {

          console.error(
            'Failed to create candidate',
            error
          );

          this.submitting = false;

          this.submitError = apiErrorMessage(error);
        }

      });
  }

  cancel(): void {

    if (
      this.isEditMode &&
      this.candidateId
    ) {

      void this.router.navigate([
        '/candidates',
        this.candidateId
      ]);

      return;
    }

    void this.router.navigate([
      '/candidates'
    ]);
  }
}
