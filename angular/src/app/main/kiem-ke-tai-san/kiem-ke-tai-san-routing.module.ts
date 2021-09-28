import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { KiemKeTaiSanComponent } from './kiem-ke-tai-san.component';

const routes: Routes = [{ path: '', component: KiemKeTaiSanComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class KiemKeTaiSanRoutingModule { }
