import { ComponentFixture, TestBed } from '@angular/core/testing';
import { JobProviderService } from '../../../core/services/job-provider.service';
import { Router } from '@angular/router';
import { createJobProviderServiceMock, RouterStub } from '../../../../test-helpers/mocks';

import { CreateJobComponent } from './create-job.component';

describe('CreateJobComponent', () => {
  let component: CreateJobComponent;
  let fixture: ComponentFixture<CreateJobComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateJobComponent],
      providers: [
        { provide: JobProviderService, useValue: createJobProviderServiceMock() },
        { provide: Router, useValue: RouterStub }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreateJobComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
