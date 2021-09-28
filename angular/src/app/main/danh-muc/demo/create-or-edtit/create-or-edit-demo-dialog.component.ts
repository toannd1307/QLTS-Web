
// tslint:disable
import { Demo_File } from '@shared/service-proxies/service-proxies';
import {
    Component,
    Injector,
    OnInit,
    EventEmitter,
    Output
} from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import * as _ from 'lodash';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { DemoServiceProxy, LookupTableServiceProxy, LookupTableDto, DemoCreateInput } from '@shared/service-proxies/service-proxies';
import * as moment from 'moment';
import { finalize } from 'rxjs/operators';
import { CommonComponent } from '@shared/dft/components/common.component';
import { forkJoin } from 'rxjs';
import { AppConsts } from '@shared/AppConsts';
import { HttpClient } from '@angular/common/http';
import { FileDownloadService } from '@shared/file-download.service';
import { AppComponentBase } from '@shared/app-component-base';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
const URL = AppConsts.remoteServiceBaseUrl + '/api/Upload/DemoUpload';
@Component({
    templateUrl: './create-or-edit-demo-dialog.component.html',
})
export class CreateOrEditDemoDialogComponent extends AppComponentBase
    implements OnInit {
    @Output() onSave = new EventEmitter<any>();
    form: FormGroup;
    saving = false;
    isEdit = false;
    demos: LookupTableDto[] = [];
    arrTrangThaiDuyet: LookupTableDto[] = [];
    arrTrangHieuLuc: LookupTableDto[] = [];
    demoDto: DemoCreateInput = new DemoCreateInput();
    id: number;
    isView: boolean;
    suggestionsSingle: LookupTableDto[];
    demoSelectedSingle: LookupTableDto;
    suggestionsMultiple: LookupTableDto[];
    demoSelectedMultiple: LookupTableDto;

    // calendar
    invalidDates: Date[]; // List date disable
    disabledDays = '[0,6]'; // Disable hết các ngày chủ nhật, thứ 7
    minDate = new Date(2020, 5, 15);
    maxDate = new Date(2020, 5, 25);
    value = 0;

    // Upload file
    uploading = false;
    maxFile = 3;
    imageFiles: File[] = [];
    allFiles: File[] = [];
    filesAllDto: Demo_File[] = [];
    returnMessage = '';
    excelAcceptTypes = '.';
    linkServer = AppConsts.remoteServiceBaseUrl;

    toChucItems: TreeviewItem[];
    loaiTaiSanItems: TreeviewItem[];
    toChucValue: number;
    loaiTaiSanValue: number;

    constructor(
        injector: Injector,
        private _fb: FormBuilder,
        public http: HttpClient,
        private _fileDownloadService: FileDownloadService,
        private _lookupTableService: LookupTableServiceProxy,
        public bsModalRef: BsModalRef,
        private _demoService: DemoServiceProxy,
    ) {
        super(injector);
    }

    logg(event) {
        console.log(99, event);
    }

    ngOnInit() {
        this.khoiTaoForm();
        forkJoin(
            this._lookupTableService.getAllTrangThaiHieuLuc(),
            this._lookupTableService.getAllTrangThaiDuyet(),
            this._lookupTableService.getAllDemo(),
            this._lookupTableService.getAllToChucTree(),
            this._lookupTableService.getAllLoaiTaiSanTree(),
        ).subscribe(([trangThaiHieuLuc, trangThaiDuyet, demo, toChuc, loaiTaiSan]) => {
            this.arrTrangHieuLuc = trangThaiHieuLuc;
            this.arrTrangThaiDuyet = trangThaiDuyet;
            console.log("in", trangThaiDuyet);
            this.demos = demo;
            this.toChucItems = this.getTreeviewItem(toChuc);
            this.loaiTaiSanItems = this.getTreeviewItem(loaiTaiSan);

            // Calendar
            const invalidDate = new Date();
            invalidDate.setDate(this.today.getDate() - 5);
            this.invalidDates = [this.today, invalidDate];

            // ProgressBar
            const interval = setInterval(() => {
                this.value = this.value + Math.floor(Math.random() * 10) + 1;
                if (this.value >= 100) {
                    this.value = 100;
                    clearInterval(interval);
                }
            }, 500);
            if (!this.id) {
                // Thêm mới
                this.demoDto = new DemoCreateInput();
                this.isEdit = false;
            } else {
                this.isEdit = true;
                // Sửa
                this._demoService.getForEdit(this.id).subscribe(item => {
                    this.demoDto = item;
                    this._setValueForEdit();
                });
            }
            if (this.isView) {
                this.form.disable();
            } else {
                this.form.enable();
            }
        });
    }

    khoiTaoForm() {
        this.form = this._fb.group({
            TextBox1: ['', Validators.required],
            TextBox2: ['', Validators.required],
            Checkbox: [],
            CheckboxTrueFalse: [],
            InputSwitch: [],
            RadioButton: [],
            InputMask: [],
            Slider: [],
            Description: [],
            InputTextarea: [],
            IntegerOnly: [],
            Decimal: [],
            DateBasic: [],
            DateTime: [],
            DateDisable: [],
            DateMinMax: [],
            DateFromTo: [],
            DateMultiple: [],
            DateMultipleMonth: [],
            MonthOnly: [],
            TimeOnly: [],
            AutoCompleteSingle: [''],
            AutoCompleteMultiple: [''],
            DropdownSingle: [],
            DropdownMultiple: [],
            DropDownTreeViewToChuc: [],
            DropDownTreeViewLoaiTaiSan: [],
        });
    }

    // Bấm tải file upload
    onDownloadFile(url) {
        this._demoService.downloadFileUpload(url).subscribe(result => {
            this._fileDownloadService.downloadTempFile(result);
        });
    }

    search(event) {
        const query = event.query;
        this._lookupTableService.getAllDemo().subscribe(result => {
            this.suggestionsMultiple = this.filterDemo(query, result);
        });
    }

    searchTrangThaiDuyet(event) {
        const query = event.query;
        this._lookupTableService.getAllTrangThaiDuyet().subscribe(result => {
            this.suggestionsSingle = this.filterTrangThaiDuyet(query, result);
        });
        console.log(this.suggestionsSingle);
    }

    filterDemo(query, demos: any[]): any[] {
        const filtered: any[] = [];
        for (const iterator of demos) {
            if (iterator.displayName.toLowerCase().indexOf(query.toLowerCase()) === 0) {
                filtered.push(iterator);
            }
        }
        return filtered;
    }

    filterTrangThaiDuyet(query, trangThai: LookupTableDto[]): any[] {
        const filtered: any[] = [];
        for (const iterator of trangThai) {
            if (iterator.displayName.toLowerCase().indexOf(query.toLowerCase()) === 0) {
                filtered.push(iterator);
            }
        }
        return filtered;
    }

    save(): void {
        if (CommonComponent.getControlErr(this.form) === '') {
            const formdata = new FormData();
            for (let i = 0; i < this.imageFiles.length; i++) {
                let item = new File([this.imageFiles[i]], this.imageFiles[i].name);
                formdata.append((i + 1) + '', item);
            }
            for (let i = 0; i < this.allFiles.length; i++) {
                let item = new File([this.allFiles[i]], this.allFiles[i].name);
                formdata.append((i + 1) + '', item);
            }
            this.http.post(URL, formdata).subscribe((res) => {
                this._getValueForSave();
                this.demoDto.listDemoFile = [];
                for (const file of this.imageFiles) {
                    const item = new Demo_File();
                    item.loaiFile = 1;
                    item.tenFile = file.name;
                    item.linkFile = this.getLinkFile(res, file.name);
                    this.demoDto.listDemoFile.push(item);
                }
                for (const file of this.allFiles) {
                    const item = new Demo_File();
                    item.loaiFile = 2;
                    item.tenFile = file.name;
                    item.linkFile = this.getLinkFile(res, file.name);
                    this.demoDto.listDemoFile.push(item);
                }
                this._demoService.createOrEdit(this.demoDto).pipe(
                    finalize(() => {
                        this.saving = false;
                    })
                ).subscribe((result) => {
                    if (result === 1) {
                        this.showExistMessage('Mã đã bị trùng!');
                    } else if (!this.id) {
                        this.showCreateMessage();
                        this.bsModalRef.hide();
                        this.onSave.emit();
                    } else {
                        this.showUpdateMessage();
                        this.bsModalRef.hide();
                        this.onSave.emit();
                    }
                });
            });
        }
    }

    onSelectAllFile(event) {
        if (!this.isView) {
            this.allFiles.push(...event.addedFiles);
        }
    }

    onRemoveAllFile(event) {
        this.allFiles.splice(this.allFiles.indexOf(event), 1);
    }

    onSelectImageFile(event) {
        if (!this.isView) {
            this.imageFiles.push(...event.addedFiles);
        }
    }

    onRemoveImageFile(event) {
        this.imageFiles.splice(this.imageFiles.indexOf(event), 1);
    }

    close() {
        this.bsModalRef.hide();
    }

    private _setValueForEdit() {
        this.form.controls.TextBox1.setValue(this.demoDto.ma);
        this.form.controls.TextBox2.setValue(this.demoDto.ten);
        this.form.controls.Checkbox.setValue(this.demoDto.checkbox ? this.demoDto.checkbox.split(this.separator) : null);
        this.form.controls.CheckboxTrueFalse.setValue(this.demoDto.checkboxTrueFalse);
        this.form.controls.RadioButton.setValue(this.demoDto.radioButton);
        this.form.controls.InputSwitch.setValue(this.demoDto.inputSwitch);
        this.form.controls.InputMask.setValue(this.demoDto.inputMask);
        this.form.controls.Slider.setValue(this.demoDto.slider);
        this.form.controls.Description.setValue(this.demoDto.description);
        this.form.controls.InputTextarea.setValue(this.demoDto.inputTextarea);
        this.form.controls.IntegerOnly.setValue(this.demoDto.integerOnly);
        this.form.controls.Decimal.setValue(this.demoDto.decimal);
        this.form.controls.DateBasic.setValue(CommonComponent.getDateForEditFromMoment(this.demoDto.dateBasic));
        this.form.controls.DateTime.setValue(CommonComponent.getDateTimeForEditFromMoment(this.demoDto.dateTime));
        this.form.controls.DateDisable.setValue(CommonComponent.getDateForEditFromMoment(this.demoDto.dateDisable));
        this.form.controls.DateMinMax.setValue(CommonComponent.getDateForEditFromMoment(this.demoDto.dateMinMax));

        if (this.demoDto.dateFrom && this.demoDto.dateTo) {
            this.form.controls.DateFromTo.setValue(
                [this.demoDto.dateFrom ? CommonComponent.getDateForEditFromMoment(this.demoDto.dateFrom) : null,
                this.demoDto.dateTo ? CommonComponent.getDateForEditFromMoment(this.demoDto.dateTo) : null]);
        }
        const listDate: Date[] = [];
        this.demoDto.dateMultiple?.split(this.separator).forEach(item => {
            listDate.push(CommonComponent.getDateForEditFromString(item));
        });
        this.form.controls.DateMultiple.setValue(listDate.length > 0 ? listDate : null);
        this.form.controls.DateMultipleMonth.setValue(CommonComponent.getDateForEditFromMoment(this.demoDto.dateMultipleMonth));
        this.form.controls.MonthOnly.setValue(
            CommonComponent.getDateForEditFromMoment(this.demoDto.monthOnly, this.dateFormatMonthOnlyInsert));
        this.form.controls.TimeOnly.setValue(CommonComponent.getTimeEditFromString(this.demoDto.timeOnly));

        this.form.controls.DropdownSingle.setValue(this.arrTrangHieuLuc.find(e => e.id === this.demoDto.dropdownSingle));
        this.form.controls.AutoCompleteSingle.setValue(this.arrTrangThaiDuyet.find(e => e.id === this.demoDto.autoCompleteSingle));
        this._lookupTableService.getAllDemo(
        ).subscribe(result => {
            const listDropdownMultiple = this.demoDto.dropdownMultiple?.split(this.separator).map(e => Number(e));
            this.form.controls.DropdownMultiple.setValue(listDropdownMultiple ?
                result.filter(e => listDropdownMultiple.findIndex(w => w === e.id) > -1) : null);
            const listAutoCompleteMultiple = this.demoDto.autoCompleteMultiple?.split(this.separator).map(e => Number(e));
            this.form.controls.AutoCompleteMultiple.setValue(listAutoCompleteMultiple ?
                result.filter(e => listAutoCompleteMultiple.findIndex(w => w === e.id) > -1) : null);
        });

        // hiển thị file
        for (const file of this.demoDto.listDemoFile) {
            const path = AppConsts.remoteServiceBaseUrl + '\\Upload\\Demo' + file.linkFile;
            this.http.get(path,
                { responseType: 'blob' }).subscribe((data) => {
                    if (file.loaiFile == 1) {
                        this.imageFiles.push(this.blobToFile(data, file.tenFile));
                    } else {
                        this.allFiles.push(this.blobToFile(data, file.tenFile));
                    }
                });
        }
    }


    private _getValueForSave() {
        this.demoDto.ma = this.form.controls.TextBox1.value;
        this.demoDto.ten = this.form.controls.TextBox2.value;
        this.demoDto.checkbox = this.form.controls.Checkbox.value ?
            this.form.controls.Checkbox.value.map(e => e).join(this.separator) : null;
        this.demoDto.checkboxTrueFalse = this.form.controls.CheckboxTrueFalse.value || false;
        this.demoDto.radioButton = this.form.controls.RadioButton.value;
        this.demoDto.inputSwitch = this.form.controls.InputSwitch.value || false;
        this.demoDto.inputMask = this.form.controls.InputMask.value;
        this.demoDto.slider = this.form.controls.Slider.value;
        this.demoDto.description = this.form.controls.Description.value;
        this.demoDto.inputTextarea = this.form.controls.InputTextarea.value;
        this.demoDto.integerOnly = this.form.controls.IntegerOnly.value;
        this.demoDto.decimal = +this.form.controls.Decimal.value;

        this.demoDto.dateBasic = this.form.controls.DateBasic.value;
        this.demoDto.dateTime = this.form.controls.DateTime.value;
        this.demoDto.dateDisable = this.form.controls.DateDisable.value;
        this.demoDto.dateMinMax = this.form.controls.DateMinMax.value;
        this.demoDto.dateFrom = this.form.controls.DateFromTo.value ? this.form.controls.DateFromTo.value[0] : null;
        this.demoDto.dateTo = this.form.controls.DateFromTo.value ? this.form.controls.DateFromTo.value[1] : null;
        this.demoDto.dateMultiple = this.form.controls.DateMultiple.value ?
            this.form.controls.DateMultiple.value?.map(e => moment(e).format(this.dateFormatInsert)).join(this.separator) : null;
        this.demoDto.dateMultipleMonth = this.form.controls.DateMultipleMonth.value;
        this.demoDto.monthOnly = this.form.controls.MonthOnly.value;
        this.demoDto.timeOnly = this.form.controls.TimeOnly.value ?
            moment(this.form.controls.TimeOnly.value).format(this.timeFormatInsert) : null;

        this.demoDto.autoCompleteSingle = this.form.controls.AutoCompleteSingle.value?.id;
        this.demoDto.autoCompleteMultiple = this.form.controls.AutoCompleteMultiple.value !== '' ?
            this.form.controls.AutoCompleteMultiple.value?.map(e => e.id).join(this.separator) : null;
        this.demoDto.dropdownSingle = this.form.controls.DropdownSingle.value?.id;
        this.demoDto.dropdownMultiple = this.form.controls.DropdownMultiple.value ?
            this.form.controls.DropdownMultiple.value?.map(e => e.id).join(this.separator) : null;

    }
}
