import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DatLichXuatBaoCaoRoutingModule } from './dat-lich-xuat-bao-cao-routing.module';
import { DatLichXuatBaoCaoComponent } from './dat-lich-xuat-bao-cao.component';
import { CreateOrEditDatLichComponent } from './create-or-edit-dat-lich/create-or-edit-dat-lich.component';
import { SharedModule } from '@shared/shared.module';
@NgModule({
  declarations: [DatLichXuatBaoCaoComponent, CreateOrEditDatLichComponent],
  imports: [
    CommonModule,
    DatLichXuatBaoCaoRoutingModule,
    SharedModule,
  ]
})
export class DatLichXuatBaoCaoModule { }
