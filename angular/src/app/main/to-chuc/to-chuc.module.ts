import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ToChucRoutingModule } from './to-chuc-routing.module';
import { ToChucComponent } from './to-chuc.component';
import { CreateOrEditToChucComponent } from './create-or-edit-to-chuc/create-or-edit-to-chuc.component';
import { SharedModule } from '@shared/shared.module';


@NgModule({
  declarations: [ToChucComponent, CreateOrEditToChucComponent],
  imports: [
    CommonModule,
    SharedModule,
    ToChucRoutingModule
  ]
})
export class ToChucModule { }
