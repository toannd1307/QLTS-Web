import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { BaoCaoCanhBaoComponent } from './bao-cao-canh-bao.component';

const routes: Routes = [{ path: '', component: BaoCaoCanhBaoComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class BaoCaoCanhBaoRoutingModule { }
