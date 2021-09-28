import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { ToChucComponent } from './to-chuc.component';

const routes: Routes = [{ path: '', component: ToChucComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ToChucRoutingModule { }
