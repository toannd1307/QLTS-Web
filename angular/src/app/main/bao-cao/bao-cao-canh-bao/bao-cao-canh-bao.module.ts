import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { BaoCaoCanhBaoRoutingModule } from './bao-cao-canh-bao-routing.module';
import { BaoCaoCanhBaoComponent } from './bao-cao-canh-bao.component';
import { SharedModule } from '@shared/shared.module';



@NgModule({
  declarations: [BaoCaoCanhBaoComponent],
  imports: [
    CommonModule,
    SharedModule,
    BaoCaoCanhBaoRoutingModule
  ]
})
export class BaoCaoCanhBaoModule { }
