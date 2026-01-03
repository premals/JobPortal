import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JobProviderDashboardComponent } from './job-provider-dashboard.component';

describe('JobProviderDashboardComponent', () => {
  let component: JobProviderDashboardComponent;
  let fixture: ComponentFixture<JobProviderDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JobProviderDashboardComponent]
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
