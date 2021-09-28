import { LookupTableServiceProxy, TaiSanChuaSuDungForViewDto, TaiSanChuaSuDungServiceProxy, TaiSanDangSuDungServiceProxy } from './../../../../../../shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/app-component-base';
import { Component, EventEmitter, Injector, OnInit, Output, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { LookupTableDto } from '@shared/service-proxies/service-proxies';
import { forkJoin } from 'rxjs';
import { LazyLoadEvent } from 'primeng/api';
import { finalize } from 'rxjs/operators';
import { CommonComponent } from '@shared/dft/components/common.component';
import { Table } from 'primeng/table';

@Component({
  selector: 'app-them-cap-phat-tai-san',
  templateUrl: './them-cap-phat-tai-san.component.html',
  styleUrls: ['./them-cap-phat-tai-san.component.scss']
})
export class ThemCapPhatTaiSanComponent extends AppComponentBase implements OnInit {

  form: FormGroup;
  loading = true;
  flag: number;
  keyword = '';
  saving = false;
  loaiTaiSanValue: number;
  totalCount: number;
  listToChucId: number[];

  loaiTaiSanItems: TreeviewItem[];
  nhaCungCaps: LookupTableDto[];
  records: TaiSanChuaSuDungForViewDto[] = [];
  arrTaiSanCheckedFromParent: TaiSanChuaSuDungForViewDto[] = [];
  arrTaiSanChecked: TaiSanChuaSuDungForViewDto[] = [];

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
    forkJoin(
      this._lookupTableService.getAllLoaiTaiSanTree(),
      this._lookupTableService.getAllNhaCungCap(),
    ).subscribe(([loaiTaiSan, nhaCungCap]) => {
      this.loaiTaiSanItems = this.getTreeviewItem(loaiTaiSan);
      this.nhaCungCaps = nhaCungCap;
    });
  }

  khoiTaoForm(): void {
    this.form = this._fb.group({
      NhaCungCap: [],
    });
  }

  close(): void {
    this.bsModalRef.hide();
  }

  searchTrangThaiNhaCungCap(event) {
    const query = event.query;
    this._lookupTableService.getAllNhaCungCap().subscribe(result => {
      this.nhaCungCaps = this.filterTrangThai(query, result);
    });
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
      this._taiSanChuaSuDungServiceProxy.getAllTaiSanCapPhat(
        this.keyword || undefined,
        undefined,
        this.loaiTaiSanValue || undefined,
        this.form.controls.NhaCungCap.value?.id,
        undefined,
        isSearch,
        this.getSortField(this.table),
        lazyLoad ? lazyLoad.first : this.table.first,
        lazyLoad ? lazyLoad.rows : this.table.rows,
      ).pipe(finalize(() => { this.loading = false; }))
        .subscribe(result => {
          this.setValueForRecords(result);
        });
      this.loading = false;
    }

    // flag = 2 tài sản đang sử dụng
    if (this.flag === 2) {
      this._taiSanDangSuDungServiceProxy.getAllTaiSanCapPhat(
        this.keyword || undefined,
        undefined,
        this.loaiTaiSanValue || undefined,
        this.form.controls.NhaCungCap.value?.id,
        undefined,
        isSearch,
        this.getSortField(this.table),
        lazyLoad ? lazyLoad.first : this.table.first,
        lazyLoad ? lazyLoad.rows : this.table.rows,
      ).pipe(finalize(() => { this.loading = false; }))
        .subscribe(result => {
          this.setValueForRecords(result);
        });
      this.loading = false;
    }
    this.loading = false;
  }
  setValueForRecords(result: any) {
    if (!this.permission.isGranted('Pages.QuanLyTaiSanTong')) {
      result.items = result.items.filter(f => !this.arrTaiSanCheckedFromParent
        .map(e => e.id).includes(f.id) && f.capPhat && this.listToChucId.includes(f.toChucId));
    } else {
      result.items = result.items.filter(f => !this.arrTaiSanCheckedFromParent
        .map(e => e.id).includes(f.id) && f.capPhat);
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
