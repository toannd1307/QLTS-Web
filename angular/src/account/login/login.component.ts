import { Component, Injector, ViewChild } from '@angular/core';
import { AbpSessionService } from 'abp-ng2-module';
import { AppComponentBase } from '@shared/app-component-base';
import { accountModuleAnimation } from '@shared/animations/routerTransition';
import { AppAuthService } from '@shared/auth/app-auth.service';

@Component({
  templateUrl: './login.component.html',
  animations: [accountModuleAnimation()]
})
export class LoginComponent extends AppComponentBase {
  submitting = false;
  fieldTextType: boolean;
  @ViewChild('passwordEl') password;

  constructor(
    injector: Injector,
    public authService: AppAuthService,
    private _sessionService: AbpSessionService
  ) {
    super(injector);
  }

  get multiTenancySideIsTeanant(): boolean {
    return this._sessionService.tenantId > 0;
  }

  get isSelfRegistrationAllowed(): boolean {
    if (!this._sessionService.tenantId) {
      return false;
    }

    return true;
  }

  login(): void {
    this.submitting = true;
    this.authService.authenticate(() => {
      this.submitting = false;
      abp.utils.setCookieValue(
        'taiSanTabIndexActive',
        '0',
        new Date(this.today.setDate(this.today.getDate() + 1)),
        abp.appPath
      );
    });
  }

  toggleFieldTextType() {
    if (this.fieldTextType) {
      this.password.nativeElement.type = "password";
    } else {
      this.password.nativeElement.type = "text";
    }
    this.fieldTextType = !this.fieldTextType;
  }
}
