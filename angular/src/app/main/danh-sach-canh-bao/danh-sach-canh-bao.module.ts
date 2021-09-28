import { SharedModule } from './../../../shared/shared.module';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DanhSachCanhBaoRoutingModule } from './danh-sach-canh-bao-routing.module';
import { DanhSachCanhBaoComponent } from './danh-sach-canh-bao.component';


@NgModule({
  declarations: [DanhSachCanhBaoComponent],
  imports: [
    CommonModule,
    SharedModule,
    DanhSachCanhBaoRoutingModule
  ]
})
export class DanhSachCanhBaoModule { }
