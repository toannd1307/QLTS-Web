import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { BaoCaoThongTinTaiSanRoutingModule } from './bao-cao-thong-tin-tai-san-routing.module';
import { BaoCaoThongTinTaiSanComponent } from './bao-cao-thong-tin-tai-san.component';
import { SharedModule } from '@shared/shared.module';


@NgModule({
  declarations: [BaoCaoThongTinTaiSanComponent],
  imports: [
    CommonModule,
    SharedModule,
    BaoCaoThongTinTaiSanRoutingModule
  ]
})
export class BaoCaoThongTinTaiSanModule { }
