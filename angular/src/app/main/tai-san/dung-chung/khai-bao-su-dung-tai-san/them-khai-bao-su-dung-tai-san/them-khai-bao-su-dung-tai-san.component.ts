import { finalize } from 'rxjs/operators';
import { AppComponentBase } from '@shared/app-component-base';
import { Component, EventEmitter, OnInit, Output, ViewChild, Injector } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { LookupTableServiceProxy, TaiSanChuaSuDungForViewDto, TaiSanChuaSuDungServiceProxy, LookupTableDto } from '@shared/service-proxies/service-proxies';
import { Table } from 'primeng/table';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { forkJoin } from 'rxjs';
import { LazyLoadEvent } from 'primeng/api';
import { CommonComponent } from '@shared/dft/components/common.component';
import { PermissionTreeEditModel } from '@shared/dft/components/permission-tree-edit.model';

@Component({
  selector: 'app-them-khai-bao-su-dung-tai-san',
  templateUrl: './them-khai-bao-su-dung-tai-san.component.html',
  styleUrls: ['./them-khai-bao-su-dung-tai-san.component.scss']
})
export class ThemKhaiBaoSuDungTaiSanComponent extends AppComponentBase implements OnInit {

  form: FormGroup;
  loading = true;
  flag: number;
  keyword = '';
  saving = false;
  loaiTaiSanValue: number;
  totalCount: number;
  toChucValue: number[];
  listToChucId: number[];

  loaiTaiSanItems: TreeviewItem[];
  records: TaiSanChuaSuDungForViewDto[] = [];
  arrTaiSanCheckedFromParent: TaiSanChuaSuDungForViewDto[] = [];
  arrTaiSanChecked: TaiSanChuaSuDungForViewDto[] = [];
  toChucItems: PermissionTreeEditModel;

  @Output() recordsForCheck: TaiSanChuaSuDungForViewDto[] = [];
  @Output() onSave = new EventEmitter<any>();
  @ViewChild('dt') table: Table;


  constructor(
    injector: Injector,
    public bsModalRef: BsModalRef,
    private _lookupTableService: LookupTableServiceProxy,
    private _fb: FormBuilder,
    private _taiSanChuaSuDungServiceProxy: TaiSanChuaSuDungServiceProxy
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.khoiTaoForm();
    this._lookupTableService.getAllToChucIdTheoNguoiDungList(true).subscribe(rs =>
      this.listToChucId = rs
    );

    this._lookupTableService.getAllToChucTheoNguoiDung().subscribe((data) => {
      this.toChucItems = {
        data,
        selectedData: data.map(e => e.id),
      };
      this.toChucValue = data.map(e => e.id);
      this.getDataPage(false);
    });

    forkJoin(
      this._lookupTableService.getAllLoaiTaiSanTree(),
    ).subscribe(([loaiTaiSan]) => {
      this.loaiTaiSanItems = this.getTreeviewItem(loaiTaiSan);
    });
  }

  khoiTaoForm(): void {
    this.form = this._fb.group({
    });
  }

  close(): void {
    this.bsModalRef.hide();
  }


  filterTrangThai(query, trangThai: LookupTableDto[]): any[] {
    const filtered: any[] = [];
    for (const iterator of trangThai) {
      if (iterator.displayName.toLowerCase().indexOf(query.toLowerCase()) === 0) {
        filtered.push(iterator);
      }
    }
    return filtered;
  }

  getDataPage(isSearch: boolean, lazyLoad?: LazyLoadEvent) {
    this.loading = true;
    // flag = 1 tài sản chưa sử dụng
    if (this.flag === 1) {
      this._taiSanChuaSuDungServiceProxy.getAllByMaAndSerialNumber(
        this.keyword || undefined,
        this.toChucValue || undefined,
        this.loaiTaiSanValue || undefined,
        undefined,
        undefined,
        isSearch,
        this.getSortField(this.table),
        lazyLoad ? lazyLoad.first : this.table.first,
        lazyLoad ? lazyLoad.rows : this.table.rows,
      ).pipe(finalize(() => { this.loading = false; }))
        .subscribe(result => {
          result.items = result.items.filter(f => !this.arrTaiSanCheckedFromParent
            .some(s => s.id === f.id));
          if (this.toChucValue === undefined && !this.permission.isGranted('Pages.QuanLyTaiSanTong')) {
            result.items = result.items.filter(f => this.listToChucId.includes(f.toChucId));
          }
          this.records = result.items;
          this.totalCount = result.totalCount;
        });
      this.loading = false;
    }

    // flag = 2 tài sản đang sử dụng
    if (this.flag === 2) {

    }
    this.loading = false;
  }
  save(): void {
    if (CommonComponent.getControlErr(this.form) === '') {
      this.saving = true;
      this.bsModalRef.hide();
      this.onSave.emit();
    }
  }
}
