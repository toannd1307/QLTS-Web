import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { AngtenRfidComponent } from './angten-rfid.component';
import { GiamSatTaiSanComponent } from './giam-sat-tai-san/giam-sat-tai-san.component';
import { TheoDoiKetNoiThietBiComponent } from './theo-doi-ket-noi-thiet-bi/theo-doi-ket-noi-thiet-bi.component';

const routes: Routes = [
  { path: 'dau-doc-co-dinh', component: AngtenRfidComponent },
  { path: 'giam-sat-tai-san', component: GiamSatTaiSanComponent },
  { path: 'theo-doi-ket-noi-thiet-bi', component: TheoDoiKetNoiThietBiComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AngtenRfidRoutingModule { }
