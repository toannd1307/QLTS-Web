import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { DemoComponent } from './demo/demo.component';
import { AppRouteGuard } from '../../../shared/auth/auth-route-guard';
@NgModule({
    imports: [
        RouterModule.forChild([
            {
                path: 'demo', component: DemoComponent,
                canActivate: [AppRouteGuard]
            },
        ])
    ],
    exports: [RouterModule],
    declarations: [],
    providers: [],
})
export class DanhMucRoutingModule { }
