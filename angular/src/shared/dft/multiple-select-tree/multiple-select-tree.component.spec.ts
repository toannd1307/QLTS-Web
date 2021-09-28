import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MultipleSelectTreeComponent } from './multiple-select-tree.component';

describe('MultipleSelectTreeComponent', () => {
  let component: MultipleSelectTreeComponent;
  let fixture: ComponentFixture<MultipleSelectTreeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MultipleSelectTreeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MultipleSelectTreeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
