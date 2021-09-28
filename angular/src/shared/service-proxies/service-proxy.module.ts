import { NgModule } from '@angular/core';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { AbpHttpInterceptor } from 'abp-ng2-module';

import * as ApiServiceProxies from './service-proxies';

@NgModule({
    providers: [
        ApiServiceProxies.RoleServiceProxy,
        ApiServiceProxies.SessionServiceProxy,
        ApiServiceProxies.TenantServiceProxy,
        ApiServiceProxies.UserServiceProxy,
        ApiServiceProxies.AuditLogServiceProxy,
        ApiServiceProxies.TokenAuthServiceProxy,
        ApiServiceProxies.AccountServiceProxy,
        ApiServiceProxies.ConfigurationServiceProxy,
        ApiServiceProxies.DemoServiceProxy,
        ApiServiceProxies.LookupTableServiceProxy,
        ApiServiceProxies.NhaCungCapServiceProxy,
        ApiServiceProxies.ToChucServiceProxy,
        ApiServiceProxies.DauDocTheRFIDServiceProxy,
        ApiServiceProxies.AngTenRFIDServiceProxy,
        ApiServiceProxies.LoaiTaiSanServiceProxy,
        ApiServiceProxies.ViTriDiaLyServiceProxy,
        ApiServiceProxies.ToanBoTaiSanServiceProxy,
        ApiServiceProxies.KhaiBaoHongMatServiceProxy,
        ApiServiceProxies.TaiSanChuaSuDungServiceProxy,
        ApiServiceProxies.TaiSanDangSuDungServiceProxy,
        ApiServiceProxies.TaiSanHongServiceProxy,
        ApiServiceProxies.TaiSanHuyServiceProxy,
        ApiServiceProxies.TaiSanMatServiceProxy,
        ApiServiceProxies.TaiSanSuaChuaBaoDuongServiceProxy,
        ApiServiceProxies.TaiSanThanhLyServiceProxy,
        ApiServiceProxies.KiemKeTaiSanServiceProxy,
        ApiServiceProxies.MailServerServiceProxy,
        ApiServiceProxies.BaoCaoNguoiDungServiceProxy,
        ApiServiceProxies.DatLichXuatBaoCaoServiceProxy,
        ApiServiceProxies.BaoCaoCanhBaoServiceProxy,
        ApiServiceProxies.BaoCaoThongTinThietBiRFIDServiceProxy,
        ApiServiceProxies.BaoCaoThongTinTaiSanServiceProxy,
        ApiServiceProxies.TaiSanChuaSuDungServiceProxy,
        ApiServiceProxies.CanhBaoServiceProxy,
        ApiServiceProxies.LichSuRaVaoAngtenServiceProxy,
        ApiServiceProxies.BaoCaoCanhBaoServiceProxy,
        ApiServiceProxies.PhieuDuTruMuaSamServiceProxy,
        { provide: HTTP_INTERCEPTORS, useClass: AbpHttpInterceptor, multi: true }
    ]
})
export class ServiceProxyModule { }
