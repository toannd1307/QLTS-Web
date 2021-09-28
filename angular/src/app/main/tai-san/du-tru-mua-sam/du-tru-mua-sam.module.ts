import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DuTruMuaSamRoutingModule } from './du-tru-mua-sam-routing.module';
import { DuTruMuaSamComponent } from './du-tru-mua-sam.component';
import { CreateOrEditDuTruMuaSamComponent } from './create-or-edit-du-tru-mua-sam/create-or-edit-du-tru-mua-sam.component';
import { SharedModule } from '@shared/shared.module';
import { PhieuDuTruMuaSamComponent } from './create-or-edit-du-tru-mua-sam/phieu-du-tru-mua-sam/phieu-du-tru-mua-sam.component';


@NgModule({
  declarations: [DuTruMuaSamComponent, CreateOrEditDuTruMuaSamComponent, PhieuDuTruMuaSamComponent],
  imports: [
    CommonModule,
    SharedModule,
    DuTruMuaSamRoutingModule
  ]
})
export class DuTruMuaSamModule { }
