import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaoCaoRoutingModule } from './bao-cao-routing.module';
import { BaoCaoComponent } from './bao-cao.component';
import { SharedModule } from '@shared/shared.module';


@NgModule({
  declarations: [BaoCaoComponent],
  imports: [
    CommonModule,
    SharedModule,
    BaoCaoRoutingModule
  ]
})
export class BaoCaoModule { }
