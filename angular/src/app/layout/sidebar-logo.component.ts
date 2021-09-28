// tslint:disable
import { BreakpointObserver, Breakpoints, BreakpointState } from '@angular/cdk/layout';
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { OnInit } from '@angular/core';
import { LayoutStoreService } from '@shared/layout/layout-store.service';

@Component({
  selector: 'sidebar-logo',
  templateUrl: './sidebar-logo.component.html',
  styles: [`
  .mobile-close{
    color:red;
    position:absolute;
    top:1rem;
    right:1rem;
    background-color: #343a40 !important
  }
  .close-item{
    font-size: 2rem;
    color: #fff;
  }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SidebarLogoComponent implements OnInit {
  isMobile = false;

  constructor
    (public breakpointObserver: BreakpointObserver,
      private _layoutStore: LayoutStoreService) {

  }
  ngOnInit(): void {
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
          this.isMobile = true;
        }
        else if (state.breakpoints[Breakpoints.Medium] || state.breakpoints[Breakpoints.Large] || state.breakpoints[Breakpoints.XLarge]) {
          this.isMobile = false;
        }
      });
  }

  close() {
    this._layoutStore.setSidebarExpanded(true);
  }
}
