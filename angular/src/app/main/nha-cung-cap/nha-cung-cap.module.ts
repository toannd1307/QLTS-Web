import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { NhaCungCapRoutingModule } from './nha-cung-cap-routing.module';
import { NhaCungCapComponent } from './nha-cung-cap.component';
import { CreateOrEditNhaCungCapComponent } from './create-or-edit-nha-cung-cap/create-or-edit-nha-cung-cap.component';
import { SharedModule } from '@shared/shared.module';


@NgModule({
  declarations: [NhaCungCapComponent, CreateOrEditNhaCungCapComponent],
  imports: [
    CommonModule,
    SharedModule,
    NhaCungCapRoutingModule,
  ]
})
export class NhaCungCapModule { }
