// tslint:disable
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { TokenService, LogService, UtilsService } from 'abp-ng2-module';
import { AppConsts } from '@shared/AppConsts';
import { UrlHelper } from '@shared/helpers/UrlHelper';
import {
    AuthenticateModel,
    AuthenticateResultModel,
    TokenAuthServiceProxy,
    IAuthenticateSSOModel,
    AuthenticateSSOResultModel,
    ExternalAuthenticateModel,
    ExternalAuthenticateResultModel,
} from '@shared/service-proxies/service-proxies';

@Injectable()
export class AppAuthService {
    externalAuthenticateModel: ExternalAuthenticateModel;
    authenticateModel: AuthenticateModel;
    authenticateResult: AuthenticateResultModel;
    externalAuthenticateResult: ExternalAuthenticateResultModel;
    rememberMe: boolean;

    constructor(
        private _tokenAuthService: TokenAuthServiceProxy,
        private _router: Router,
        private _utilsService: UtilsService,
        private _tokenService: TokenService,
        private _logService: LogService
    ) {
        this.clear();
    }

    loginCallback(data?: IAuthenticateSSOModel, finallyCallback?: () => void): void {
        finallyCallback = finallyCallback || (() => { });
        this._tokenAuthService.loginSSO(data).subscribe((result: AuthenticateSSOResultModel) => {
            this._tokenAuthService.externalAuthenticate(new ExternalAuthenticateModel({
                providerAccessCode: "Mobifone",
                providerKey: result.id,
                authProvider: "Mobifone"
            }))
            .pipe(
                finalize(() => {
                    finallyCallback();
                })
            )
            .subscribe((result: ExternalAuthenticateResultModel) => {
                this.processExternalAuthenticateResult(result);
            });
        });
    }

    logout(reload?: boolean): void {
        this._tokenAuthService.logout().subscribe(() => {
            abp.auth.clearToken();
            abp.utils.setCookieValue(
                AppConsts.authorization.encryptedAuthTokenName,
                undefined,
                undefined,
                abp.appPath
            );
            if (reload !== false) {
                // sso
                location.href = 'https://devsso.smart-office.vn/sso/logout?client_id=SMART_OFFICE_DEV&redirect_uri=https://qlts.mobifone.vn';
                // location.href = AppConsts.appBaseUrl;
            }
        });
    }

    authenticate(finallyCallback?: () => void): void {
        finallyCallback = finallyCallback || (() => { });
        this._tokenAuthService
            .authenticate(this.authenticateModel)
            .pipe(
                finalize(() => {
                    finallyCallback();
                })
            )
            .subscribe((result: AuthenticateResultModel) => {
                this.processAuthenticateResult(result);
            });
    }

    private processAuthenticateResult(
        authenticateResult: AuthenticateResultModel
    ) {
        this.authenticateResult = authenticateResult;

        if (authenticateResult.accessToken) {
            // Successfully logged in
            this.login(
                authenticateResult.accessToken,
                authenticateResult.encryptedAccessToken,
                authenticateResult.expireInSeconds,
                this.rememberMe
            );
        } else {
            // Unexpected result!

            this._logService.warn('Unexpected authenticateResult!');
            this._router.navigate(['account/login']);
        }
    }

    private processExternalAuthenticateResult(
        externalAuthenticateResult: ExternalAuthenticateResultModel
    ) {
        this.externalAuthenticateResult = externalAuthenticateResult;

        if (externalAuthenticateResult.accessToken) {
            // Successfully logged in
            this.login(
                externalAuthenticateResult.accessToken,
                externalAuthenticateResult.encryptedAccessToken,
                externalAuthenticateResult.expireInSeconds,
                this.rememberMe
            );
        } else {
            // Unexpected result!

            this._logService.warn('Unexpected authenticateResult!');
            this._router.navigate(['account/login']);
        }
    }

    private login(
        accessToken: string,
        encryptedAccessToken: string,
        expireInSeconds: number,
        rememberMe?: boolean
    ): void {
        const tokenExpireDate = rememberMe
            ? new Date(new Date().getTime() + 1000 * expireInSeconds)
            : undefined;

        this._tokenService.setToken(accessToken, tokenExpireDate);

        this._utilsService.setCookieValue(
            AppConsts.authorization.encryptedAuthTokenName,
            encryptedAccessToken,
            tokenExpireDate,
            abp.appPath
        );

        let initialUrl = UrlHelper.initialUrl;
        if (initialUrl.indexOf('/login') > 0 || initialUrl.indexOf('/callback') > 0) {
            initialUrl = AppConsts.appBaseUrl;
        }

        location.href = initialUrl;
    }

    private clear(): void {
        this.authenticateModel = new AuthenticateModel();
        this.authenticateModel.rememberClient = false;
        this.authenticateResult = null;
        this.rememberMe = false;
    }
}
