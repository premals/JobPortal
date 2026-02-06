import { of } from 'rxjs';

export const createAuthServiceMock = () => ({
  register: (_: any) => of(void 0),
  login: (_: any) => of({ accessToken: '', refreshToken: '', profile: {} }),
  forgotPassword: (_: any) => of(void 0),
  resetPassword: (_: any) => of(void 0),
  refreshToken: () => of({ accessToken: '', refreshToken: '', profile: {} }),
  logout: () => {}
});

export const createJobSeekerServiceMock = () => ({
  getJobs: () => of([]),
  getJob: (_: any) => of({}),
  apply: (_: any) => of(void 0)
});

export const createJobProviderServiceMock = () => ({
  getMyJobs: () => of([]),
  createJob: (_: any) => of(void 0),
  getJob: (_: any) => of({}),
  getJobById: (_: any) => of({ keySkills: [], expiryDate: '' }),
  updateJob: (_id: any, _payload: any) => of(void 0),
  getApplicationsByJob: (_: any) => of([])
});

export const createProfileServiceMock = () => ({
  getProfile: () => of({}),
  updateProfile: (_: any) => of({})
});

export const ActivatedRouteStub = {
  snapshot: { queryParamMap: { get: (_: string) => null }, paramMap: { get: (_: string) => null }, params: {}, queryParams: {} },
  queryParams: of({}),
  params: of({})
};

export const RouterStub = {
  navigate: (..._args: any[]) => Promise.resolve(true),
  events: of({}),
  createUrlTree: (..._args: any[]) => ({}),
  serializeUrl: (_: any) => ''
};
