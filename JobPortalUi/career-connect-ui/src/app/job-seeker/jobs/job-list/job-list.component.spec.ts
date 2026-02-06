import { ComponentFixture, TestBed } from '@angular/core/testing';
import { JobSeekerService } from '../../../core/services/job-seeker.service';
import { createJobSeekerServiceMock } from '../../../../test-helpers/mocks';

import { JobListComponent } from './job-list.component';

describe('JobListComponent', () => {
  let component: JobListComponent;
  let fixture: ComponentFixture<JobListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JobListComponent],
      providers: [
        { provide: JobSeekerService, useValue: createJobSeekerServiceMock() }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
