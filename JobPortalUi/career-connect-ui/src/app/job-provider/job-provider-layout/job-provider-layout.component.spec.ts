import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JobProviderLayoutComponent } from './job-provider-layout.component';

describe('JobProviderLayoutComponent', () => {
  let component: JobProviderLayoutComponent;
  let fixture: ComponentFixture<JobProviderLayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JobProviderLayoutComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobProviderLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
