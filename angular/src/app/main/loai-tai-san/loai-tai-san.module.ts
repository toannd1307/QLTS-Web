import { SharedModule } from './../../../shared/shared.module';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { LoaiTaiSanRoutingModule } from './loai-tai-san-routing.module';
import { LoaiTaiSanComponent } from './loai-tai-san.component';
import { CreateOrEditLoaiTaiSanComponent } from './create-or-edit-loai-tai-san/create-or-edit-loai-tai-san.component';


@NgModule({
  declarations: [LoaiTaiSanComponent, CreateOrEditLoaiTaiSanComponent],
  imports: [
    CommonModule,
    SharedModule,
    LoaiTaiSanRoutingModule
  ]
})
export class LoaiTaiSanModule { }
