import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AngtenRfidRoutingModule } from './angten-rfid-routing.module';
import { AngtenRfidComponent } from './angten-rfid.component';
import { SharedModule } from '@shared/shared.module';
import { GiamSatTaiSanComponent } from './giam-sat-tai-san/giam-sat-tai-san.component';
import { TheoDoiKetNoiThietBiComponent } from './theo-doi-ket-noi-thiet-bi/theo-doi-ket-noi-thiet-bi.component';
import { CreateOrEditAngtenRfidComponent } from './create-or-edit/create-or-edit-angten-rfid/create-or-edit-angten-rfid.component';


@NgModule({
  declarations: [AngtenRfidComponent, GiamSatTaiSanComponent, TheoDoiKetNoiThietBiComponent, CreateOrEditAngtenRfidComponent],
  imports: [
    CommonModule,
    SharedModule,
    AngtenRfidRoutingModule
  ]
})
export class AngtenRfidModule { }
