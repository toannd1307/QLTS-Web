import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { ViTriDiaLyComponent } from './vi-tri-dia-ly.component';

const routes: Routes = [{ path: '', component: ViTriDiaLyComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ViTriDiaLyRoutingModule { }
