import { Component, Injector, ChangeDetectionStrategy } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FooterComponent extends AppComponentBase {
  currentYear: number;
  versionText: string;

  constructor(injector: Injector) {
    super(injector);
    this.currentYear = new Date().getFullYear();

    // DFT
    this.versionText = '1.1.23.52 [14012021]';

    // Khách hàng
    this.versionText = '1.1.23 [27012021]';

  }
}
