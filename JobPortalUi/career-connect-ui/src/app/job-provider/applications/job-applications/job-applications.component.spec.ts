import { ComponentFixture, TestBed } from '@angular/core/testing';
import { JobProviderService } from '../../../core/services/job-provider.service';
import { ActivatedRoute } from '@angular/router';
import { createJobProviderServiceMock, ActivatedRouteStub } from '../../../../test-helpers/mocks';

import { JobApplicationsComponent } from './job-applications.component';

describe('JobApplicationsComponent', () => {
  let component: JobApplicationsComponent;
  let fixture: ComponentFixture<JobApplicationsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JobApplicationsComponent],
      providers: [
        { provide: JobProviderService, useValue: createJobProviderServiceMock() },
        { provide: ActivatedRoute, useValue: ActivatedRouteStub }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobApplicationsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
