import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { BaoCaoNguoiDungComponent } from './bao-cao-nguoi-dung.component';

const routes: Routes = [{ path: '', component: BaoCaoNguoiDungComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class BaoCaoNguoiDungRoutingModule { }
