import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AppRouteGuard } from '@shared/auth/auth-route-guard';
import { TaiSanComponent } from './tai-san.component';

const routes: Routes = [
  { path: '', component: TaiSanComponent, canActivate: [AppRouteGuard] },
  { path: 'du-tru-mua-sam', loadChildren: () => import('./du-tru-mua-sam/du-tru-mua-sam.module').then(m => m.DuTruMuaSamModule) },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TaiSanRoutingModule { }
