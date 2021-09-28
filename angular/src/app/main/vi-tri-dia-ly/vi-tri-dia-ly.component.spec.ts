import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ViTriDiaLyComponent } from './vi-tri-dia-ly.component';

describe('ViTriDiaLyComponent', () => {
  let component: ViTriDiaLyComponent;
  let fixture: ComponentFixture<ViTriDiaLyComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ViTriDiaLyComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ViTriDiaLyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
