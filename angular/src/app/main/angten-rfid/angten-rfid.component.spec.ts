import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AngtenRfidComponent } from './angten-rfid.component';

describe('AngtenRfidComponent', () => {
  let component: AngtenRfidComponent;
  let fixture: ComponentFixture<AngtenRfidComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AngtenRfidComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AngtenRfidComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
