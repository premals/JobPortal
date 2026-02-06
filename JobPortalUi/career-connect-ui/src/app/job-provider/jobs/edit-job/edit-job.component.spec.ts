import { ComponentFixture, TestBed } from '@angular/core/testing';
import { JobProviderService } from '../../../core/services/job-provider.service';
import { Router, ActivatedRoute } from '@angular/router';
import { createJobProviderServiceMock, RouterStub, ActivatedRouteStub } from '../../../../test-helpers/mocks';

import { EditJobComponent } from './edit-job.component';

describe('EditJobComponent', () => {
  let component: EditJobComponent;
  let fixture: ComponentFixture<EditJobComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditJobComponent],
      providers: [
        { provide: JobProviderService, useValue: createJobProviderServiceMock() },
        { provide: Router, useValue: RouterStub },
        { provide: ActivatedRoute, useValue: ActivatedRouteStub }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditJobComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
