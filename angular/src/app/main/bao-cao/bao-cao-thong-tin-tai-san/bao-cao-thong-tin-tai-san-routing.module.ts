import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { BaoCaoThongTinTaiSanComponent } from './bao-cao-thong-tin-tai-san.component';

const routes: Routes = [{ path: '', component: BaoCaoThongTinTaiSanComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class BaoCaoThongTinTaiSanRoutingModule { }
