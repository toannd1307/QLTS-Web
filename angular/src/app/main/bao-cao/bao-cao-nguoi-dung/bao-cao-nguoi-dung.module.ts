import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { BaoCaoNguoiDungRoutingModule } from './bao-cao-nguoi-dung-routing.module';
import { BaoCaoNguoiDungComponent } from './bao-cao-nguoi-dung.component';
import { SharedModule } from '@shared/shared.module';

@NgModule({
  declarations: [BaoCaoNguoiDungComponent],
  imports: [
    CommonModule,
    BaoCaoNguoiDungRoutingModule,
    SharedModule
  ]
})
export class BaoCaoNguoiDungModule { }
