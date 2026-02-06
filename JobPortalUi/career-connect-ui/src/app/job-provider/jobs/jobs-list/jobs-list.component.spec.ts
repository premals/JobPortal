import { ComponentFixture, TestBed } from '@angular/core/testing';
import { JobProviderService } from '../../../core/services/job-provider.service';
import { Router, ActivatedRoute } from '@angular/router';
import { createJobProviderServiceMock, RouterStub, ActivatedRouteStub } from '../../../../test-helpers/mocks';

import { JobProviderJobsListComponent } from './jobs-list.component';

describe('JobProviderJobsListComponent', () => {
  let component: JobProviderJobsListComponent;
  let fixture: ComponentFixture<JobProviderJobsListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JobProviderJobsListComponent],
      providers: [
        { provide: JobProviderService, useValue: createJobProviderServiceMock() },
        { provide: Router, useValue: RouterStub },
        { provide: ActivatedRoute, useValue: ActivatedRouteStub }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobProviderJobsListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
