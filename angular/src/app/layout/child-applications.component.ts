import { AppComponentBase } from '@shared/app-component-base';
// tslint:disable
import {
  Component,
  ChangeDetectionStrategy,
  Renderer2,
  OnInit,
  Injector
} from '@angular/core';
import { LayoutStoreService } from '@shared/layout/layout-store.service';
import { BreakpointState, Breakpoints, BreakpointObserver } from '@angular/cdk/layout';
import { UserServiceProxy } from '@shared/service-proxies/service-proxies';

@Component({
  selector: 'childApplications',
  templateUrl: './child-applications.component.html',
})
export class ChildApplicationsComponent extends AppComponentBase implements OnInit {
  childAppsExpanded: boolean;
  isMobile = false;
  constructor(
    injector: Injector,
    private renderer: Renderer2,
    private _layoutStore: LayoutStoreService,
    public breakpointObserver: BreakpointObserver
  ) {
    super(injector);
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

      this._layoutStore.childAppsExpanded.subscribe((value) => {
        this.childAppsExpanded = value;
        this.toggleChildApps();
      });

      console.log(this.appSession.childApps);
  }

  closeChildApps() {
    this._layoutStore.setChildAppsExpanded(!this.childAppsExpanded);
    this._layoutStore.setSidebarExpanded(true);
  }

  toggleChildApps(): void {
    if (this.childAppsExpanded) {
      this.hideChildApps();
    } else {
      this.showChildApps();
    }
  }

  showChildApps(): void {
    //this.renderer.addClass(document.body, 'child-applications-open');
    $('.blur-body').addClass('child-applications-open');
    $('.content-wrapper').addClass(this.isMobile ? 'm-r-500' : 'm-r-250');
    $('.content-wrapper').removeClass('m-r-0');
    $('.main-header').removeClass('m-r-0');
    $('.main-header').addClass(this.isMobile ? 'm-r-500' : 'm-r-250');
    $('.main-childApps').removeClass('visibility-hidden');
    $('.main-childApps').addClass('bg-transparent');
    $('.main-childApps').addClass(this.isMobile ? 'width100' : 'width-250');
  }

  hideChildApps(): void {
    //this.renderer.removeClass(document.body, 'child-applications-open');
    $('.blur-body').removeClass('child-applications-open');
    $('.content-wrapper').removeClass(this.isMobile ? 'm-r-500' : 'm-r-250');
    $('.content-wrapper').addClass('m-r-0');
    $('.main-header').removeClass(this.isMobile ? 'm-r-500' : 'm-r-250');
    $('.main-header').addClass('m-r-0');
    $('.main-childApps').addClass('visibility-hidden');
    $('.main-childApps').removeClass(this.isMobile ? 'width100' : 'width-250');
  }
}
