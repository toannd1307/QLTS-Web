import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { BaoCaoThongTinThietBiRfidRoutingModule } from './bao-cao-thong-tin-thiet-bi-rfid-routing.module';
import { BaoCaoThongTinThietBiRfidComponent } from './bao-cao-thong-tin-thiet-bi-rfid.component';
import { SharedModule } from '@shared/shared.module';


@NgModule({
  declarations: [BaoCaoThongTinThietBiRfidComponent],
  imports: [
    CommonModule,
    BaoCaoThongTinThietBiRfidRoutingModule,
    SharedModule,
  ]
})
export class BaoCaoThongTinThietBiRfidModule { }
