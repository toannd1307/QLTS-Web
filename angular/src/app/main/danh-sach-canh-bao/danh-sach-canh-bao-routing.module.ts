import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { DanhSachCanhBaoComponent } from './danh-sach-canh-bao.component';

const routes: Routes = [{ path: '', component: DanhSachCanhBaoComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DanhSachCanhBaoRoutingModule { }
