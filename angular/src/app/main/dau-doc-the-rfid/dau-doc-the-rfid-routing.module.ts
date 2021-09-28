import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { DauDocTheRfidComponent } from './dau-doc-the-rfid.component';

const routes: Routes = [{ path: '', component: DauDocTheRfidComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DauDocTheRfidRoutingModule { }
