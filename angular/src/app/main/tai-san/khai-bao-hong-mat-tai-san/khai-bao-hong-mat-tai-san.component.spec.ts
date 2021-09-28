import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { KhaiBaoHongMatTaiSanComponent } from './khai-bao-hong-mat-tai-san.component';

describe('KhaiBaoHongMatTaiSanComponent', () => {
  let component: KhaiBaoHongMatTaiSanComponent;
  let fixture: ComponentFixture<KhaiBaoHongMatTaiSanComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ KhaiBaoHongMatTaiSanComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(KhaiBaoHongMatTaiSanComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
