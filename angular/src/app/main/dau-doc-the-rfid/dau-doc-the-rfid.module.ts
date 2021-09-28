import { SharedModule } from './../../../shared/shared.module';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DauDocTheRfidRoutingModule } from './dau-doc-the-rfid-routing.module';
import { DauDocTheRfidComponent } from './dau-doc-the-rfid.component';
import { CreateOrEditDauDocTheRfidComponent } from './create-or-edit/create-or-edit-dau-doc-the-rfid/create-or-edit-dau-doc-the-rfid.component';


@NgModule({
  declarations: [DauDocTheRfidComponent, CreateOrEditDauDocTheRfidComponent],
  imports: [
    CommonModule,
    SharedModule,
    DauDocTheRfidRoutingModule
  ]
})
export class DauDocTheRfidModule { }
