import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { LoaiTaiSanComponent } from './loai-tai-san.component';

const routes: Routes = [{ path: '', component: LoaiTaiSanComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LoaiTaiSanRoutingModule { }
