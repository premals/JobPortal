Testing Guidelines

Quick notes for writing unit tests in this repo:

- Avoid importing heavy modules (like `HttpClientTestingModule` or `RouterTestingModule`) into every spec. Prefer providing minimal, focused mocks for faster and more isolated tests.

- Helpers are available in `src/test-helpers/mocks.ts`:
  - `createAuthServiceMock()`, `createJobSeekerServiceMock()`, `createJobProviderServiceMock()`, `createProfileServiceMock()` — factories returning simple observables
  - `RouterStub`, `ActivatedRouteStub` — simple router/route stubs useful in specs

- Example (in component spec):

  import { TestBed } from '@angular/core/testing';
  import { AuthService } from '../../core/services/auth.service';
  import { createAuthServiceMock, RouterStub } from '../../../test-helpers/mocks';

  await TestBed.configureTestingModule({
    imports: [MyComponent],
    providers: [
      { provide: AuthService, useValue: createAuthServiceMock() },
      { provide: Router, useValue: RouterStub }
    ]
  }).compileComponents();

- When you do need full HTTP-level testing (rare), you can still import `HttpClientTestingModule` directly in the spec.

Thanks — please keep tests fast and focused! 👏