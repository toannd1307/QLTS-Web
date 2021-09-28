import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { BaoCaoCanhBaoComponent } from './bao-cao-canh-bao.component';

describe('BaoCaoCanhBaoComponent', () => {
  let component: BaoCaoCanhBaoComponent;
  let fixture: ComponentFixture<BaoCaoCanhBaoComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ BaoCaoCanhBaoComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BaoCaoCanhBaoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
