import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
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
  CandidateListItem,
  CandidateService
} from '../../../candidates/services/candidate.service';

import {
  CreateInterviewRequest,
  InterviewService,
  UpdateInterviewRequest
} from '../../services/interview.service';

@Component({
  selector: 'app-interview-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './interview-form.component.html',
  styleUrl: './interview-form.component.scss'
})
export class InterviewFormComponent implements OnInit {

  candidates: CandidateListItem[] = [];

  isEditMode = false;
  interviewId: string | null = null;

  loading = true;
  submitting = false;
  submitError = false;

  interviewTypes = [
    { value: 1, label: 'Technical' },
    { value: 2, label: 'System Design' },
    { value: 3, label: 'Behavioral' }
  ];

  levels = [
    { value: 1, label: 'Junior' },
    { value: 2, label: 'Mid' },
    { value: 3, label: 'Senior' },
    { value: 4, label: 'Lead' }
  ];

  interviewForm;

  constructor(
    private readonly fb: FormBuilder,
    private readonly router: Router,
    private readonly route: ActivatedRoute,
    private readonly candidateService: CandidateService,
    private readonly interviewService: InterviewService
  ) {
    this.interviewForm = this.fb.nonNullable.group({
      candidateId: ['', Validators.required],

      title: [
        '',
        [
          Validators.required,
          Validators.maxLength(100)
        ]
      ],

      type: [1, Validators.required],
      level: [2, Validators.required],

      date: ['', Validators.required],
      time: ['', Validators.required],

      durationMinutes: [
        60,
        [
          Validators.required,
          Validators.min(15),
          Validators.max(240)
        ]
      ],

      topics: [''],

      notes: [
        '',
        Validators.maxLength(1000)
      ]
    });
  }

  ngOnInit(): void {
    this.interviewId =
      this.route.snapshot.paramMap.get('id');

    this.isEditMode = !!this.interviewId;

    this.loadCandidates();
  }

  private loadCandidates(): void {
    this.candidateService
      .getCandidates()
      .subscribe({
        next: candidates => {
          this.candidates = candidates;

          if (
            this.isEditMode &&
            this.interviewId
          ) {
            this.loadInterview(
              this.interviewId
            );
          } else {
            this.loading = false;
          }
        },
        error: () => {
          this.loading = false;
          this.submitError = true;
        }
      });
  }

  private loadInterview(id: string): void {
    this.interviewService
      .getInterview(id)
      .subscribe({
        next: interview => {

          const scheduledAt =
            new Date(interview.scheduledAt);

          const year =
            scheduledAt.getFullYear();

          const month =
            String(
              scheduledAt.getMonth() + 1
            ).padStart(2, '0');

          const day =
            String(
              scheduledAt.getDate()
            ).padStart(2, '0');

          const hours =
            String(
              scheduledAt.getHours()
            ).padStart(2, '0');

          const minutes =
            String(
              scheduledAt.getMinutes()
            ).padStart(2, '0');

          this.interviewForm.patchValue({
            candidateId:
            interview.candidateId,

            title:
            interview.title,

            type:
            interview.type,

            level:
            interview.level,

            date:
              `${year}-${month}-${day}`,

            time:
              `${hours}:${minutes}`,

            durationMinutes:
            interview.durationMinutes,

            topics:
              interview.topics ?? '',

            notes:
              interview.notes ?? ''
          });

          this.interviewForm.controls
            .candidateId.disable();

          this.loading = false;
        },
        error: () => {
          this.loading = false;
          this.submitError = true;
        }
      });
  }

  submit(): void {
    if (this.interviewForm.invalid) {
      this.interviewForm.markAllAsTouched();
      return;
    }

    this.submitting = true;
    this.submitError = false;

    const value =
      this.interviewForm.getRawValue();

    const scheduledAt =
      new Date(
        `${value.date}T${value.time}:00`
      ).toISOString();

    if (
      this.isEditMode &&
      this.interviewId
    ) {
      const request:
        UpdateInterviewRequest = {

        title: value.title.trim(),
        type: value.type,
        level: value.level,
        scheduledAt,
        durationMinutes:
        value.durationMinutes,

        topics:
          value.topics.trim() || null,

        notes:
          value.notes.trim() || null
      };

      this.interviewService
        .updateInterview(
          this.interviewId,
          request
        )
        .subscribe({
          next: () => {
            this.router.navigate([
              '/interviews',
              this.interviewId
            ]);
          },
          error: () => {
            this.submitting = false;
            this.submitError = true;
          }
        });

      return;
    }

    const request:
      CreateInterviewRequest = {

      candidateId:
      value.candidateId,

      title:
        value.title.trim(),

      type:
      value.type,

      level:
      value.level,

      scheduledAt,

      durationMinutes:
      value.durationMinutes,

      topics:
        value.topics.trim() || null,

      notes:
        value.notes.trim() || null
    };

    this.interviewService
      .createInterview(request)
      .subscribe({
        next: interview => {
          this.router.navigate([
            '/interviews',
            interview.id
          ]);
        },
        error: () => {
          this.submitting = false;
          this.submitError = true;
        }
      });
  }

  cancel(): void {
    if (
      this.isEditMode &&
      this.interviewId
    ) {
      this.router.navigate([
        '/interviews',
        this.interviewId
      ]);

      return;
    }

    this.router.navigate([
      '/interviews'
    ]);
  }
}
