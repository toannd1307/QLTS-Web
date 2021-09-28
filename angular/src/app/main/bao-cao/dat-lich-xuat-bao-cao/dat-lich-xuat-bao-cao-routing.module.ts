import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { DatLichXuatBaoCaoComponent } from './dat-lich-xuat-bao-cao.component';

const routes: Routes = [{ path: '', component: DatLichXuatBaoCaoComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DatLichXuatBaoCaoRoutingModule { }
