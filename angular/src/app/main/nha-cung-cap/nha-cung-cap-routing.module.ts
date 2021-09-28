import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { NhaCungCapComponent } from './nha-cung-cap.component';

const routes: Routes = [{ path: '', component: NhaCungCapComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class NhaCungCapRoutingModule { }
