import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { NEVER } from 'rxjs';
import { InterviewFormComponent } from './features/interviewer/interviews/pages/interview-form/interview-form.component';
import { FeedbackFormComponent } from './features/interviewer/feedback/pages/feedback-form/feedback-form.component';
import { CandidateFormComponent } from './features/interviewer/candidates/pages/candidate-form/candidate-form.component';
import { CandidateService } from './features/interviewer/candidates/services/candidate.service';

describe('Application form validation', () => {
  beforeEach(() => TestBed.configureTestingModule({
    providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()]
  }));

  it('accepts interview duration endpoints and rejects out-of-range or fractional values', () => {
    const form = TestBed.createComponent(InterviewFormComponent).componentInstance.interviewForm;
    for (const value of [14, 241, 15.5]) {
      form.controls.durationMinutes.setValue(value);
      expect(form.controls.durationMinutes.valid).withContext(String(value)).toBeFalse();
    }
    for (const value of [15, 240]) {
      form.controls.durationMinutes.setValue(value);
      expect(form.controls.durationMinutes.valid).withContext(String(value)).toBeTrue();
    }
    form.controls.title.setValue('   ');
    expect(form.controls.title.valid).toBeFalse();
  });

  it('accepts feedback score endpoints and rejects out-of-range or fractional values', () => {
    const form = TestBed.createComponent(FeedbackFormComponent).componentInstance.feedbackForm;
    for (const value of [0, 11, 1.5]) {
      form.controls.overallScore.setValue(value);
      expect(form.controls.overallScore.valid).withContext(String(value)).toBeFalse();
    }
    for (const value of [1, 10]) {
      form.controls.overallScore.setValue(value);
      expect(form.controls.overallScore.valid).withContext(String(value)).toBeTrue();
    }
    form.controls.strengths.setValue('   ');
    expect(form.controls.strengths.valid).toBeFalse();
  });

  it('submits a numeric experience level and empty optional target role after selecting an option', () => {
    const create = spyOn(TestBed.inject(CandidateService), 'createCandidate').and.returnValue(NEVER);
    const fixture = TestBed.createComponent(CandidateFormComponent);
    fixture.detectChanges();
    fixture.componentInstance.candidateForm.patchValue({ firstName: 'Test', lastName: 'Candidate', email: 'test@example.com' });
    const select: HTMLSelectElement = fixture.nativeElement.querySelector('#experienceLevel');
    select.selectedIndex = 2;
    select.dispatchEvent(new Event('change'));
    fixture.componentInstance.submit();
    fixture.componentInstance.submit();
    expect(create).toHaveBeenCalledTimes(1);
    expect(create.calls.mostRecent().args[0]).toEqual(jasmine.objectContaining({ experienceLevel: 3, targetRole: '' }));
    fixture.destroy();
  });
});
