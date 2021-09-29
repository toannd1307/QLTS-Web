import {ActivatedRoute, Router} from '@angular/router';
import {OnInit, Component} from '@angular/core';
import {
    IAuthenticateSSOModel
} from '@shared/service-proxies/service-proxies';
import { AppAuthService } from '@shared/auth/app-auth.service';
import { AppComponentBase } from '@shared/app-component-base';
@Component({
    template: ''
})
export class CallbackComponent implements OnInit  {
    constructor(
        private route: ActivatedRoute, 
        private router: Router,
        private _authService: AppAuthService) {}

    ngOnInit() {
        this.route.fragment.subscribe(fragment => {
           const token_type =  new URLSearchParams(fragment).get('token_type');
           const id_token =  new URLSearchParams(fragment).get('id_token');
           const refresh_token =  new URLSearchParams(fragment).get('refresh_token');
           const session_id =  new URLSearchParams(fragment).get('sessionId');

           const data: IAuthenticateSSOModel = ({
               Authorization: 'Bearer' + ' ' + id_token,
               SessionId: session_id,
               TenantCode: 'mobifone.vn',
           });

           this._authService.loginCallback(data, () => {
            abp.utils.setCookieValue(
                'taiSanTabIndexActive',
                '0',
                new Date(new Date().setDate(new Date().getDate() + 1)),
                abp.appPath
              );
           });
        });
    }
}