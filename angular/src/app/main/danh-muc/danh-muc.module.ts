import { DanhMucRoutingModule } from './danh-muc-routing.module';
import { SharedModule } from '../../../shared/shared.module';
import { NgModule } from '@angular/core';
import { DemoComponent } from './demo/demo.component';
import { CreateOrEditDemoDialogComponent } from './demo/create-or-edtit/create-or-edit-demo-dialog.component';

@NgModule({
    imports: [
        SharedModule,
        DanhMucRoutingModule
    ],
    declarations: [
        DemoComponent,
        CreateOrEditDemoDialogComponent,
    ],
})
export class DanhMucModule { }
