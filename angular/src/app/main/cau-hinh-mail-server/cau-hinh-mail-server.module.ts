import { SharedModule } from './../../../shared/shared.module';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { CauHinhMailServerRoutingModule } from './cau-hinh-mail-server-routing.module';
import { CauHinhMailServerComponent } from './cau-hinh-mail-server.component';


@NgModule({
  declarations: [CauHinhMailServerComponent],
  imports: [
    CommonModule,
    SharedModule,
    CauHinhMailServerRoutingModule
  ]
})
export class CauHinhMailServerModule { }
