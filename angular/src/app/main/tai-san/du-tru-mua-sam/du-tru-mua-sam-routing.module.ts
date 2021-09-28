import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { DuTruMuaSamComponent } from './du-tru-mua-sam.component';

const routes: Routes = [{ path: '', component: DuTruMuaSamComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DuTruMuaSamRoutingModule { }
