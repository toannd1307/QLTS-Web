import { Injectable } from '@angular/core';
import { PermissionCheckerService } from 'abp-ng2-module';
import { AppSessionService } from '../session/app-session.service';

import {
    CanActivate, Router,
    ActivatedRouteSnapshot,
    RouterStateSnapshot,
    CanActivateChild,
} from '@angular/router';

@Injectable()
export class AppRouteGuard implements CanActivate, CanActivateChild {

    constructor(
        private _permissionChecker: PermissionCheckerService,
        private _router: Router,
        private _sessionService: AppSessionService,
    ) { }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
        if (!this._sessionService.user) {
            // sso
            location.href = 'https://devsso.smart-office.vn/sso/authorize-spa?response_type=id_token%20token&scope=openid&nonce=anything&state=anything&client_id=SMART_OFFICE_DEV&redirect_uri=https://qlts.mobifone.vn/account/callback&tenant_code=10'
            // this._router.navigate(['/account/login']);
            return false;
        }

        if (!route.data || !route.data['permission']) {
            return true;
        }

        if (this._permissionChecker.isGranted(route.data['permission'])) {
            return true;
        }

        this._router.navigate([this.selectBestRoute()]);
        return false;
    }

    canActivateChild(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
        return this.canActivate(route, state);
    }

    selectBestRoute(): string {
        if (!this._sessionService.user) {
            // sso
            return location.href = 'https://devsso.smart-office.vn/sso/authorize-spa?response_type=id_token%20token&scope=openid&nonce=anything&state=anything&client_id=SMART_OFFICE_DEV&redirect_uri=https://qlts.mobifone.vn/account/callback&tenant_code=10';
            // return '/account/login';
        }

        if (this._permissionChecker.isGranted('Pages.Users')) {
            return '/app/admin/users';
        }

        return '/app/main/tai-san';
    }
}
