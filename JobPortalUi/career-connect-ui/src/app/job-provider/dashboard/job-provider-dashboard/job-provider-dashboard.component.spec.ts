import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { RouterStub } from '../../../../test-helpers/mocks';

import { JobProviderDashboardComponent } from './job-provider-dashboard.component';

describe('JobProviderDashboardComponent', () => {
  let component: JobProviderDashboardComponent;
  let fixture: ComponentFixture<JobProviderDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JobProviderDashboardComponent],
      providers: [
        { provide: Router, useValue: RouterStub }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobProviderDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
