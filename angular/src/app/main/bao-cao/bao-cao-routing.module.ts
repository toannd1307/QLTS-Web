import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { BaoCaoComponent } from './bao-cao.component';

const routes: Routes = [{ path: '', component: BaoCaoComponent },
{
  path: 'bao-cao-nguoi-dung',
  loadChildren: () => import('./bao-cao-nguoi-dung/bao-cao-nguoi-dung.module').then(m => m.BaoCaoNguoiDungModule)
},
{
  path: 'dat-lich-xuat-bao-cao',
  loadChildren: () => import('./dat-lich-xuat-bao-cao/dat-lich-xuat-bao-cao.module').then(m => m.DatLichXuatBaoCaoModule)
},
{
  path: 'bao-cao-canh-bao',
  loadChildren: () => import('./bao-cao-canh-bao/bao-cao-canh-bao.module').then(m => m.BaoCaoCanhBaoModule)
},
{
  path: 'bao-cao-thong-tin-thiet-bi-rfid',
  loadChildren: () => import('./bao-cao-thong-tin-thiet-bi-rfid/bao-cao-thong-tin-thiet-bi-rfid.module')
    .then(m => m.BaoCaoThongTinThietBiRfidModule)
},
{
  path: 'bao-cao-thong-tin-tai-san',
  loadChildren: () => import('./bao-cao-thong-tin-tai-san/bao-cao-thong-tin-tai-san.module').then(m => m.BaoCaoThongTinTaiSanModule)
}];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class BaoCaoRoutingModule { }
