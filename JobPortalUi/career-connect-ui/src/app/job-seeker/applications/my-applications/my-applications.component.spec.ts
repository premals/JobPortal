import { ComponentFixture, TestBed } from '@angular/core/testing';
import { JobSeekerService } from '../../../core/services/job-seeker.service';
import { createJobSeekerServiceMock } from '../../../../test-helpers/mocks';

import { MyApplicationsComponent } from './my-applications.component';

describe('MyApplicationsComponent', () => {
  let component: MyApplicationsComponent;
  let fixture: ComponentFixture<MyApplicationsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyApplicationsComponent],
      providers: [
        { provide: JobSeekerService, useValue: createJobSeekerServiceMock() }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyApplicationsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
