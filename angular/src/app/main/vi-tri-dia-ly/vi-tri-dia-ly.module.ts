import { SharedModule } from './../../../shared/shared.module';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ViTriDiaLyRoutingModule } from './vi-tri-dia-ly-routing.module';
import { ViTriDiaLyComponent } from './vi-tri-dia-ly.component';
import { CreateOrEditVitriDialyComponent } from './create-or-edit-vitri-dialy/create-or-edit-vitri-dialy.component';
import { AppModule } from '@app/app.module';


@NgModule({
  declarations: [ViTriDiaLyComponent, CreateOrEditVitriDialyComponent],
  imports: [
    CommonModule,
    SharedModule,
    ViTriDiaLyRoutingModule,
    AppModule,
  ]
})
export class ViTriDiaLyModule { }
