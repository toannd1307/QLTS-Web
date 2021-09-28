import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { BaoCaoThongTinThietBiRfidComponent } from './bao-cao-thong-tin-thiet-bi-rfid.component';

const routes: Routes = [{ path: '', component: BaoCaoThongTinThietBiRfidComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class BaoCaoThongTinThietBiRfidRoutingModule { }
