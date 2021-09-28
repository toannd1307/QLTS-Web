import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { BaoCaoNguoiDungComponent } from './bao-cao-nguoi-dung.component';

describe('BaoCaoNguoiDungComponent', () => {
  let component: BaoCaoNguoiDungComponent;
  let fixture: ComponentFixture<BaoCaoNguoiDungComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ BaoCaoNguoiDungComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BaoCaoNguoiDungComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
