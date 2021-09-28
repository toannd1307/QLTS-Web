import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { BaoCaoThongTinThietBiRfidComponent } from './bao-cao-thong-tin-thiet-bi-rfid.component';

describe('BaoCaoThongTinThietBiRfidComponent', () => {
  let component: BaoCaoThongTinThietBiRfidComponent;
  let fixture: ComponentFixture<BaoCaoThongTinThietBiRfidComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ BaoCaoThongTinThietBiRfidComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BaoCaoThongTinThietBiRfidComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
