import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AiResumeComponent } from './ai-resume.component';

describe('AiResumeComponent', () => {
  let component: AiResumeComponent;
  let fixture: ComponentFixture<AiResumeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AiResumeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AiResumeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
