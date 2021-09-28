// tslint:disable
import { CommonComponent } from '../../shared/dft/components/common.component';
import { Component, ChangeDetectionStrategy, OnInit } from '@angular/core';
import { LayoutStoreService,  } from '@shared/layout/layout-store.service';
import { BreakpointObserver, Breakpoints, BreakpointState } from '@angular/cdk/layout';

@Component({
  selector: 'header-left-navbar',
  templateUrl: './header-left-navbar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HeaderLeftNavbarComponent implements OnInit {
  sidebarExpanded: boolean;
  testmenu = false;

  constructor(private _layoutStore: LayoutStoreService,
    public breakpointObserver: BreakpointObserver) { }

  ngOnInit(): void {
    this._layoutStore.sidebarExpanded.subscribe((value) => {
      this.sidebarExpanded = value;
    });
    this.sidebarExpanded = true;

    this.breakpointObserver
    .observe([
      Breakpoints.XSmall,
      Breakpoints.Small,
      Breakpoints.Medium,
      Breakpoints.Large,
      Breakpoints.XLarge,
    ])
    .subscribe((state: BreakpointState) => {
      if (state.breakpoints[Breakpoints.XSmall] || state.breakpoints[Breakpoints.Small]) {
        this.testmenu = false;
      }
      else if (state.breakpoints[Breakpoints.Medium] || state.breakpoints[Breakpoints.Large] || state.breakpoints[Breakpoints.XLarge]) {
        this.testmenu = true;
      }
    });
  }

  toggleSidebar(): void {
    this._layoutStore.setSidebarExpanded(!this.sidebarExpanded);
  }
}
