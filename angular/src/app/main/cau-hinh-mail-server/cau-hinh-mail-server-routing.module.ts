import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { CauHinhMailServerComponent } from './cau-hinh-mail-server.component';

const routes: Routes = [{ path: '', component: CauHinhMailServerComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CauHinhMailServerRoutingModule { }
