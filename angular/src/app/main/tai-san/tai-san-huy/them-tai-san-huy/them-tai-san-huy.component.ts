
import { Component, EventEmitter, Injector, OnInit, Output, ViewChild, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AppComponentBase } from '@shared/app-component-base';
import { CommonComponent } from '@shared/dft/components/common.component';
import { LookupTableDto, LookupTableServiceProxy, NhaCungCapGetAllInputDto, ViewTaiSanHuy, ViewTaiSan, TaiSanHuyServiceProxy } from '@shared/service-proxies/service-proxies';
import { finalize } from 'rxjs/operators';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';
import { BsModalRef } from 'ngx-bootstrap/modal';
import * as Enums from '@shared/AppEnums';
import * as Models from '@shared/AppModels';
import { forkJoin, Subscription } from 'rxjs';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { PermissionTreeEditModel } from '@shared/dft/components/permission-tree-edit.model';

@Component({
  selector: 'app-them-tai-san-huy',
  templateUrl: './them-tai-san-huy.component.html',
  styleUrls: ['./them-tai-san-huy.component.scss'],
  animations: [appModuleAnimation()],
})
export class ThemTaiSanHuyComponent extends AppComponentBase implements OnInit, OnDestroy {

  enums = Enums;
  models = Models;

  @Output() onSave = new EventEmitter<any>();
  form: FormGroup;
  saving = false;
  isEdit = false;
  id: number;
  isView = false;
  createInputDto: any;
  @ViewChild('dt') table: Table;
  loading = true;
  exporting = false;
  keyword = '';
  totalCount = 0;
  records: ViewTaiSan[] = [];
  input: NhaCungCapGetAllInputDto;
  suggestionsSingle: LookupTableDto[];

  toChucValue: number[];

  toChucItems: PermissionTreeEditModel;
  loaiTaiSanItems: TreeviewItem[];

  arrTaiSanCheckedFromParent: ViewTaiSanHuy[] = [];
  @Output() arrTaiSanChecked: ViewTaiSanHuy[] = [];

  private _unsubscribe: Subscription[] = [];

  constructor(
    injector: Injector,
    private _taiSanServiceProxy: TaiSanHuyServiceProxy,
    private _lookupTableService: LookupTableServiceProxy,
    private _fb: FormBuilder,
    public bsModalRef: BsModalRef,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.formCreate();
    this.bindingData();
    this._lookupTableService.getAllToChucTheoNguoiDung().subscribe((data) => {
      this.toChucItems = {
        data,
        selectedData: data.map(e => e.id),
      };
      this.toChucValue = data.map(e => e.id);
      this.getDataPage(false);
    });
  }

  bindingData() {
    forkJoin(
      this._lookupTableService.getAllLoaiTaiSanTree(),
    ).subscribe(([loaiTaiSan]) => {
      this.loaiTaiSanItems = this.getTreeviewItem(loaiTaiSan);
    });
  }

  ngOnDestroy(): void {
    this._unsubscribe.forEach((sb) => sb.unsubscribe());
  }

  getDataPage(isSearch: boolean, lazyLoad?: LazyLoadEvent) {
    this.loading = true;
    this._taiSanServiceProxy.getAllTaiSan(
      this.form.value.tenTaiSan || undefined,
      this.toChucValue || undefined,
      this.form.value.loaiTaiSan || undefined,
      isSearch,
      this.getSortField(this.table),
      lazyLoad ? lazyLoad.first : this.table.first,
      lazyLoad ? lazyLoad.rows : this.table.rows,
    ).pipe(
      finalize(() => { this.loading = false; })
    )
      .subscribe(result => {
        // dungch - bỏ các record đã tồn tại ở bản ghi cha
        result.items = result.items.filter(f => !this.arrTaiSanCheckedFromParent.some(s => s.id === f.id));
        this.records = result.items;
        this.totalCount = result.totalCount;
      });
  }

  formCreate() {
    this.form = this._fb.group({
      tenTaiSan: [''],
      loaiTaiSan: [''],
      phongBanQuanLy: ['']
    });
  }

  filter(query, items: LookupTableDto[]): any[] {
    const filtered: any[] = [];
    for (const iterator of items) {
      if (iterator.displayName.toLowerCase().indexOf(query.toLowerCase()) === 0) {
        filtered.push(iterator);
      }
    }
    return filtered;
  }

  save(): void {
    if (CommonComponent.getControlErr(this.form) === '') {
      this.saving = true;
      this.showCreateMessage();
      this.bsModalRef.hide();
      this.onSave.emit();
    }
  }

  close() {
    this.bsModalRef.hide();
  }
}

