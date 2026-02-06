import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, ActivatedRoute } from '@angular/router';
import { RouterStub, ActivatedRouteStub } from '../../../test-helpers/mocks';

import { JobProviderLayoutComponent } from './job-provider-layout.component';

describe('JobProviderLayoutComponent', () => {
  let component: JobProviderLayoutComponent;
  let fixture: ComponentFixture<JobProviderLayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JobProviderLayoutComponent],
      providers: [
        { provide: Router, useValue: RouterStub },
        { provide: ActivatedRoute, useValue: ActivatedRouteStub }
      ]
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
