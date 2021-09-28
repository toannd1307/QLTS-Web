import { Component, EventEmitter, Injector, OnInit, Output, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AppComponentBase } from '@shared/app-component-base';
import { CommonComponent } from '@shared/dft/components/common.component';
import {
  LookupTableDto, LookupTableServiceProxy, TaiSanChuaSuDungForViewDto,
  TaiSanChuaSuDungServiceProxy, TaiSanDangSuDungServiceProxy
} from '@shared/service-proxies/service-proxies';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { PermissionTreeEditModel } from '@shared/dft/components/permission-tree-edit.model';

@Component({
  selector: 'app-them-thu-hoi-tai-san',
  templateUrl: './them-thu-hoi-tai-san.component.html',
  styleUrls: ['./them-thu-hoi-tai-san.component.scss']
})
export class ThemThuHoiTaiSanComponent extends AppComponentBase implements OnInit {
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
    private _taiSanChuaSuDungServiceProxy: TaiSanChuaSuDungServiceProxy,
    private _taiSanDangSuDungServiceProxy: TaiSanDangSuDungServiceProxy
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

    this._lookupTableService.getAllLoaiTaiSanTree().subscribe((loaiTaiSan) => {
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
          this.setValueForRecords(result);
        });
    }

    // flag = 2 tài sản đang sử dụng
    if (this.flag === 2) {
      this._taiSanDangSuDungServiceProxy.getAllByMaAndSerialNumberTsdsd(
        this.keyword,
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
          this.setValueForRecords(result);
        });
    }
    this.loading = false;
  }

  setValueForRecords(result: any) {
    if (!this.permission.isGranted('Pages.QuanLyTaiSanTong')) {
      result.items = result.items.filter(f => !this.arrTaiSanCheckedFromParent
        .map(e => e.id).includes(f.id) && f.thuHoi && this.listToChucId.includes(f.toChucId));
    } else {
      result.items = result.items.filter(f => !this.arrTaiSanCheckedFromParent
        .map(e => e.id).includes(f.id) && f.thuHoi);
    }
    this.records = result.items;
    this.totalCount = result.totalCount;
  }
  save(): void {
    if (CommonComponent.getControlErr(this.form) === '') {
      this.saving = true;
      this.bsModalRef.hide();
      this.onSave.emit();
    }
  }
}
