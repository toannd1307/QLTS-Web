import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DatLichXuatBaoCaoComponent } from './dat-lich-xuat-bao-cao.component';

describe('DatLichXuatBaoCaoComponent', () => {
  let component: DatLichXuatBaoCaoComponent;
  let fixture: ComponentFixture<DatLichXuatBaoCaoComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DatLichXuatBaoCaoComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DatLichXuatBaoCaoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
