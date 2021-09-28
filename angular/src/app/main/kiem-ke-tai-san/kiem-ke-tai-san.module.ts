import { SharedModule } from './../../../shared/shared.module';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { KiemKeTaiSanRoutingModule } from './kiem-ke-tai-san-routing.module';
import { KiemKeTaiSanComponent } from './kiem-ke-tai-san.component';
import { CreateOrEditKiemKeTaiSanComponent } from './create-or-edit-kiem-ke-tai-san/create-or-edit-kiem-ke-tai-san.component';
import { ThemNguoiKiemKeTaiSanComponent } from './create-or-edit-kiem-ke-tai-san/them-nguoi-kiem-ke-tai-san/them-nguoi-kiem-ke-tai-san.component';
import { DanhSachKiemKeTaiSanComponent } from './create-or-edit-kiem-ke-tai-san/danh-sach-kiem-ke-tai-san/danh-sach-kiem-ke-tai-san.component';


@NgModule({
  declarations: [KiemKeTaiSanComponent, CreateOrEditKiemKeTaiSanComponent, DanhSachKiemKeTaiSanComponent, ThemNguoiKiemKeTaiSanComponent],
  imports: [
    CommonModule,
    SharedModule,
    KiemKeTaiSanRoutingModule
  ]
})
export class KiemKeTaiSanModule { }
