// tslint:disable
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AppAuthService } from '@shared/auth/app-auth.service';
import { FileDownloadService } from '@shared/file-download.service';
import { LayoutStoreService } from '@shared/layout/layout-store.service';
import { DemoServiceProxy } from '@shared/service-proxies/service-proxies';

@Component({
  selector: 'header-user-menu',
  templateUrl: './header-user-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HeaderUserMenuComponent {
  childAppsExpanded: boolean;
  constructor(
    private _layoutStore: LayoutStoreService,
    private _authService: AppAuthService,
    private _fileDownloadService: FileDownloadService,
    private _demoService: DemoServiceProxy,
  ) { }

  ngOnInit(): void {
    this._layoutStore.childAppsExpanded.subscribe((value) => {
      this.childAppsExpanded = value;
    });
    
    this.childAppsExpanded = true;
  }

  logout(): void {
    this._authService.logout();
  }

  iphoneAppDownload() {
    this._demoService.downloadIphoneApp().subscribe(result => {
      this._fileDownloadService.downloadTempFile(result);
    });
  }

  androidAppDownload() {
    this._demoService.downloadAndroidApp().subscribe(result => {
      this._fileDownloadService.downloadTempFile(result);
    });
  }

  toggleChildApps() {
    this._layoutStore.setChildAppsExpanded(!this.childAppsExpanded);
    this._layoutStore.setSidebarExpanded(true);
  }
}
