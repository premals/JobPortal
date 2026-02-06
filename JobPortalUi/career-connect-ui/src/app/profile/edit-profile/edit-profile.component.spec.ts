import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProfileService } from '../../core/services/profile.service';
import { createProfileServiceMock } from '../../../test-helpers/mocks';

import { EditProfileComponent } from './edit-profile.component';

describe('EditProfileComponent', () => {
  let component: EditProfileComponent;
  let fixture: ComponentFixture<EditProfileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditProfileComponent],
      providers: [
        { provide: ProfileService, useValue: createProfileServiceMock() }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditProfileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
