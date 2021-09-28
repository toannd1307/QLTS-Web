import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { BaoCaoThongTinTaiSanComponent } from './bao-cao-thong-tin-tai-san.component';

describe('BaoCaoThongTinTaiSanComponent', () => {
  let component: BaoCaoThongTinTaiSanComponent;
  let fixture: ComponentFixture<BaoCaoThongTinTaiSanComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ BaoCaoThongTinTaiSanComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BaoCaoThongTinTaiSanComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
