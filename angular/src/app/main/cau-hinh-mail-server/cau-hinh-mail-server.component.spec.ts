import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CauHinhMailServerComponent } from './cau-hinh-mail-server.component';

describe('CauHinhMailServerComponent', () => {
  let component: CauHinhMailServerComponent;
  let fixture: ComponentFixture<CauHinhMailServerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CauHinhMailServerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CauHinhMailServerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
