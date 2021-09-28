import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

@NgModule({
    imports: [
        RouterModule.forChild([
            {
                path: '',
                children: [
                ],
            },
            {
                path: 'danh-muc',
                loadChildren: () => import('./danh-muc/danh-muc.module').then((m) => m.DanhMucModule),
            },
            {
                path: 'nha-cung-cap',
                loadChildren: () => import('./nha-cung-cap/nha-cung-cap.module').then(m => m.NhaCungCapModule)
            },
            {
                path: 'to-chuc',
                loadChildren: () => import('./to-chuc/to-chuc.module').then(m => m.ToChucModule)
            },
            {
                path: 'tai-san',
                loadChildren: () => import('./tai-san/tai-san.module').then(m => m.TaiSanModule)
            },
            {
                path: 'loai-tai-san',
                loadChildren: () => import('./loai-tai-san/loai-tai-san.module').then(m => m.LoaiTaiSanModule)
            },
            {
                path: 'kiem-ke-tai-san',
                loadChildren: () => import('./kiem-ke-tai-san/kiem-ke-tai-san.module').then(m => m.KiemKeTaiSanModule)
            },
            {
                path: 'bao-cao',
                loadChildren: () => import('./bao-cao/bao-cao.module').then(m => m.BaoCaoModule)
            },
            {
                path: 'danh-sach-canh-bao',
                loadChildren: () => import('./danh-sach-canh-bao/danh-sach-canh-bao.module').then(m => m.DanhSachCanhBaoModule)
            },
            {
                path: 'cau-hinh-mail-server',
                loadChildren: () => import('./cau-hinh-mail-server/cau-hinh-mail-server.module').then(m => m.CauHinhMailServerModule)
            },
            {
                path: 'dau-doc-the-rfid',
                loadChildren: () => import('./dau-doc-the-rfid/dau-doc-the-rfid.module').then(m => m.DauDocTheRfidModule)
            },
            {
                path: 'angten-rfid',
                loadChildren: () => import('./angten-rfid/angten-rfid.module').then(m => m.AngtenRfidModule)
            },
            {
                path: 'vi-tri-dia-ly',
                loadChildren: () => import('./vi-tri-dia-ly/vi-tri-dia-ly.module').then(m => m.ViTriDiaLyModule)
            },
        ]),
    ],
    exports: [RouterModule],
})
export class MainRoutingModule { }
