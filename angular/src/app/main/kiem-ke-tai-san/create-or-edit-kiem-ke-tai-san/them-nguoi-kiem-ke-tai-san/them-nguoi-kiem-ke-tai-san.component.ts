// tslint:disable
import { Component, EventEmitter, Injector, OnInit, Output, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { KiemKeTaiSanCreateInputDto, LookupTableServiceProxy, UserForViewDto } from '@shared/service-proxies/service-proxies';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';
import { finalize } from 'rxjs/operators';


@Component({
  selector: 'app-them-nguoi-kiem-ke-tai-san',
  templateUrl: './them-nguoi-kiem-ke-tai-san.component.html',
  styleUrls: ['./them-nguoi-kiem-ke-tai-san.component.scss'],
  animations: [appModuleAnimation()]
})

export class ThemNguoiKiemKeTaiSanComponent extends AppComponentBase implements OnInit {

  @ViewChild('dt') table: Table;
  form: FormGroup;
  saving = false;
  isEdit = false;
  id: number;
  isView = false;
  totalCount = 0;
  createInputDto: KiemKeTaiSanCreateInputDto = new KiemKeTaiSanCreateInputDto();
  records: UserForViewDto[] = [];
  isActive: boolean | null;
  advancedFiltersVisible = false;
  loading = true;
  toChucItems: TreeviewItem[];
  toChucValue: number;
  selectedRows: any[];
  userSave: UserForViewDto[] = [];
  public event: EventEmitter<any> = new EventEmitter();

  constructor(
    injector: Injector,
    private _fb: FormBuilder,
    private formBuilder: FormBuilder,
    public bsModalRef: BsModalRef,
    private _lookupTableService: LookupTableServiceProxy,
  ) {
    super(injector);
    this.form = formBuilder.group({
      keyword: ''
    })
  }

  ngOnInit(): void {
    this._lookupTableService.getAllToChucTree()
      .subscribe((toChuc) => {
        this.toChucItems = this.getTreeviewItem(toChuc);
      });
  }

  get keyword() { return this.form.get('keyword').value }

  getDataPage(lazyLoad?: LazyLoadEvent) {
    this.loading = true;
    this._lookupTableService.getAllNguoiDung(this.keyword, this.toChucValue
    ).pipe(finalize(() => { this.loading = false; }))
      .subscribe(result => {
        this.records = result.filter(value => !JSON.stringify(this.userSave).includes(JSON.stringify(value)));
      });
  }

  isSelected() {
    if (!this.selectedRows || this.selectedRows.length === 0) {
      return true;
    }
    return false;
  }

  triggerEvent(item: any[]) {
    this.event.emit(item);
  }
  save(): void {
    if (this.selectedRows) {
      this.bsModalRef.hide();
    }
    this.triggerEvent(this.selectedRows);
    if (!this.selectedRows) {
      this.showErrorMessage("Chọn ít nhất một người kiểm kê");
    }
  }

  close() {
    this.bsModalRef.hide();
  }

}

