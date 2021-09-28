import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateOrEditDatLichComponent } from './create-or-edit-dat-lich.component';

describe('CreateOrEditDatLichComponent', () => {
  let component: CreateOrEditDatLichComponent;
  let fixture: ComponentFixture<CreateOrEditDatLichComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreateOrEditDatLichComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreateOrEditDatLichComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
