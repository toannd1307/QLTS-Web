import { DisabledOnSelectorDirective } from './dft/dropdown-treeview-select/disabled-on-selector.directive';
import { TienTePipe } from './dft/directives/tiente.pipe';
import { DecimalDirective } from './dft/directives/decimal-only.directive';
import { NumberDirective } from './dft/directives/number-only.directive';
import { DisableControlDirective } from './dft/directives/disable-control.directive';
import { ButtonBusyDirective } from './dft/directives/button-busy.directive';
import { LabelValidationComponent } from './dft/components/lable-validate.component';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { NgModule, ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';
import { NgxPaginationModule } from 'ngx-pagination';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { AppSessionService } from './session/app-session.service';
import { AppUrlService } from './nav/app-url.service';
import { AppAuthService } from './auth/app-auth.service';
import { AppRouteGuard } from './auth/auth-route-guard';
import { LocalizePipe } from '@shared/pipes/localize.pipe';
import { ValidationComponent } from './dft/components/validation-messages.component';
import { AbpPaginationControlsComponent } from './components/pagination/abp-pagination-controls.component';
import { AbpValidationSummaryComponent } from './components/validation/abp-validation.summary.component';
import { AbpModalHeaderComponent } from './components/modal/abp-modal-header.component';
import { AbpModalFooterComponent } from './components/modal/abp-modal-footer.component';
import { LayoutStoreService } from './layout/layout-store.service';
import { BusyDirective } from './directives/busy.directive';
import { EqualValidator } from './directives/equal-validator.directive';
import { ReactiveFormsModule } from '@angular/forms';
// ng-face
import { CheckboxModule } from 'primeng/checkbox';
import { ButtonModule } from 'primeng/button';
import { CalendarModule } from 'primeng/calendar';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { MegaMenuModule } from 'primeng/megamenu';
import { MessageModule } from 'primeng/message';
import { MultiSelectModule } from 'primeng/multiselect';
import { ProgressBarModule } from 'primeng/progressbar';
import { SplitButtonModule } from 'primeng/splitbutton';
import { TableModule } from 'primeng/table';
import { TieredMenuModule } from 'primeng/tieredmenu';
import { ToastModule } from 'primeng/toast';
import { TreeTableModule } from 'primeng/treetable';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { FieldsetModule } from 'primeng/fieldset';
import { EditorModule } from 'primeng/editor';
import { InputMaskModule } from 'primeng/inputmask';
import { InputSwitchModule } from 'primeng/inputswitch';
import { InputNumberModule } from 'primeng/inputnumber';
import { SliderModule } from 'primeng/slider';
import { RadioButtonModule } from 'primeng/radiobutton';
import { FileDownloadService } from './file-download.service';
import { NgxDropzoneModule } from 'ngx-dropzone';
import { AccordionModule } from 'primeng/accordion';
import { ImportExcelDialogComponent } from './components/import-excel/import-excel-dialog.component';
import { TooltipModule } from 'primeng/tooltip';
import { TabViewModule } from 'primeng/tabview';
import { MessageService } from 'primeng/api';
import { TruncatePipe } from './dft/pipes/truncate.pipe';
import { StatusNumPipe } from './dft/pipes/statusnum.pipe';
import { StatusStrPipe } from './dft/pipes/statusstr.pipe';
import { PhoneNumberDirective } from './dft/directives/phone-number.directive';
import { TreeModule } from 'primeng/tree';
import { TreeviewModule } from './dft/dropdown-treeview-select/lib/treeview.module';
import { DropdownTreeviewSelectComponent } from './dft/dropdown-treeview-select/dropdown-treeview-select.component';
import { PermissionTreeComponent } from './dft/components/permission-tree.component';
import { MultipleSelectTreeComponent } from './dft/multiple-select-tree/multiple-select-tree.component';
import { ChartModule } from 'primeng/chart';
@NgModule({
  imports: [
    ChartModule,
    CommonModule,
    RouterModule,
    NgxPaginationModule,
    FormsModule,
    NgxDropzoneModule,
    BsDropdownModule,

    // ng-face
    CheckboxModule,
    DropdownModule,
    SplitButtonModule,
    MegaMenuModule,
    ToastModule,
    ButtonModule,
    TableModule,
    MultiSelectModule,
    CalendarModule,
    ProgressBarModule,
    TieredMenuModule,
    DialogModule,
    MessageModule,
    InputTextModule,
    TreeTableModule,
    TabsModule,
    ReactiveFormsModule,
    AutoCompleteModule,
    FieldsetModule,
    EditorModule,
    InputMaskModule,
    InputSwitchModule,
    InputNumberModule,
    SliderModule,
    RadioButtonModule,
    AccordionModule,
    TooltipModule,
    TabViewModule,
    TreeModule,
    TreeviewModule.forRoot(),
  ],
  declarations: [
    AbpPaginationControlsComponent,
    AbpValidationSummaryComponent,
    AbpModalHeaderComponent,
    AbpModalFooterComponent,
    LocalizePipe,
    BusyDirective,
    ButtonBusyDirective,
    EqualValidator,

    // Component
    ValidationComponent,
    LabelValidationComponent,
    ImportExcelDialogComponent,
    PermissionTreeComponent,

    // Directive
    NumberDirective,
    DecimalDirective,
    DisableControlDirective,
    PhoneNumberDirective,
    DisabledOnSelectorDirective,

    // Pipe
    TienTePipe,
    TruncatePipe,
    StatusNumPipe,
    StatusStrPipe,
    DropdownTreeviewSelectComponent,
    MultipleSelectTreeComponent,
  ],
  exports: [
    ChartModule,
    AbpPaginationControlsComponent,
    AbpValidationSummaryComponent,
    AbpModalHeaderComponent,
    AbpModalFooterComponent,
    LocalizePipe,
    BusyDirective,
    EqualValidator,
    ImportExcelDialogComponent,
    // module
    NgxDropzoneModule,

    // component
    ValidationComponent,
    LabelValidationComponent,
    DropdownTreeviewSelectComponent,
    PermissionTreeComponent,

    // ng-face
    FieldsetModule,
    CheckboxModule,
    DropdownModule,
    SplitButtonModule,
    MegaMenuModule,
    ToastModule,
    ButtonModule,
    TableModule,
    MultiSelectModule,
    CalendarModule,
    ProgressBarModule,
    TieredMenuModule,
    DialogModule,
    MessageModule,
    InputTextModule,
    TreeTableModule,
    FormsModule,
    CommonModule,
    TabsModule,
    ReactiveFormsModule,
    AutoCompleteModule,
    EditorModule,
    InputMaskModule,
    InputSwitchModule,
    InputNumberModule,
    SliderModule,
    BsDropdownModule,
    RadioButtonModule,
    ButtonBusyDirective,
    AccordionModule,
    TooltipModule,
    TabViewModule,
    TreeModule,
    TreeviewModule,
    MultipleSelectTreeComponent,

    // Directive
    NumberDirective,
    DecimalDirective,
    DisableControlDirective,
    PhoneNumberDirective,
    DisabledOnSelectorDirective,
    // Pipe
    TienTePipe,
    TruncatePipe,
    StatusNumPipe,
    StatusStrPipe
  ],
  providers: [FileDownloadService, MessageService],
})
export class SharedModule {
  static forRoot(): ModuleWithProviders<SharedModule> {
    return {
      ngModule: SharedModule,
      providers: [
        AppSessionService,
        AppUrlService,
        AppAuthService,
        AppRouteGuard,
        LayoutStoreService,
      ],
    };
  }
}
