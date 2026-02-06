import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProfileService } from '../../core/services/profile.service';
import { AuthService } from '../../core/services/auth.service';
import { createProfileServiceMock, createAuthServiceMock } from '../../../test-helpers/mocks';

import { ViewProfileComponent } from './view-profile.component';

describe('ViewProfileComponent', () => {
  let component: ViewProfileComponent;
  let fixture: ComponentFixture<ViewProfileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ViewProfileComponent],
      providers: [
        { provide: ProfileService, useValue: createProfileServiceMock() },
        { provide: AuthService, useValue: createAuthServiceMock() }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ViewProfileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
