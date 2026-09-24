import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ActivatedRouteSnapshot, provideRouter, Router, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { routes } from './app.routes';
import { LoginComponent } from './features/auth/pages/login/login.component';
import { AuthService } from './core/services/auth.service';
import { homeGuard } from './core/guards/home.guard';
import { candidateGuard } from './core/guards/candidate.guard';
import { interviewerGuard } from './core/guards/interviewer.guard';
import { authErrorInterceptor } from './core/interceptors/auth-error.interceptor';

describe('Public login and guarded access', () => {
  let http: HttpTestingController;
  let auth: AuthService;
  let navigate: jasmine.Spy;
  let login: jasmine.Spy;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideRouter(routes), provideHttpClient(withInterceptors([authErrorInterceptor])), provideHttpClientTesting()]
    });
    http = TestBed.inject(HttpTestingController);
    auth = TestBed.inject(AuthService);
    navigate = spyOn(TestBed.inject(Router), 'navigate').and.resolveTo(true);
    login = spyOn(auth, 'login');
  });

  afterEach(() => http.verify());

  it('keeps login public and starts Okta only after an explicit click', () => {
    const route = routes.find(route => route.path === 'login');
    expect(route?.component).toBe(LoginComponent);
    expect(route?.canActivate).toBeUndefined();
    const fixture = TestBed.createComponent(LoginComponent);
    fixture.detectChanges();
    http.expectOne('/api/auth/me').flush(null, { status: 401, statusText: 'Unauthorized' });
    fixture.detectChanges();
    expect(navigate).not.toHaveBeenCalled();
    expect(login).not.toHaveBeenCalled();
    const button: HTMLButtonElement = fixture.nativeElement.querySelector('.sign-in-button');
    expect(button.textContent).toContain('Sign in with Okta');
    expect(button.disabled).toBeFalse();
    button.click();
    expect(login).toHaveBeenCalledTimes(1);
    fixture.destroy();
  });

  it('sends unauthenticated users from each guard to Angular login, not Okta', () => {
    for (const guard of [homeGuard, candidateGuard, interviewerGuard]) {
      let result: unknown;
      const stream = TestBed.runInInjectionContext(() => guard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot));
      (stream as Observable<unknown>).subscribe(value => result = value);
      http.expectOne('/api/auth/me').flush(null, { status: 401, statusText: 'Unauthorized' });
      expect(result).toBeFalse();
      expect(navigate).toHaveBeenCalledWith(['/login'], { replaceUrl: true });
    }
    expect(navigate).toHaveBeenCalledTimes(3);
    expect(login).not.toHaveBeenCalled();
  });

  it('displays forbidden access without starting login or redirecting', () => {
    const stream = TestBed.runInInjectionContext(() => interviewerGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot));
    (stream as Observable<unknown>).subscribe(value => expect(value).toBeFalse());
    http.expectOne('/api/auth/me').flush(null, { status: 403, statusText: 'Forbidden' });
    expect(auth.accessError()).toContain('permission');
    expect(navigate).not.toHaveBeenCalled();
    expect(login).not.toHaveBeenCalled();
  });
});
