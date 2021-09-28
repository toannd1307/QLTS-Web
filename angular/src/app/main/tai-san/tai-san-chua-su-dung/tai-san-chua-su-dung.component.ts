import { element } from 'protractor';
import { Component, Injector, Input, OnChanges, OnInit, SimpleChanges, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { TrangThaiTabConst, TrangThaiTaiSanConst } from '@shared/AppEnums';
import {
  LookupTableDto, LookupTableServiceProxy,
  NhaCungCapGetAllInputDto,
  TaiSanChuaSuDungForViewDto, TaiSanChuaSuDungServiceProxy
} from '@shared/service-proxies/service-proxies';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { CapPhatTaiSanComponent } from '../dung-chung/cap-phat-tai-san/cap-phat-tai-san.component';
import { DieuChuyenTaiSanComponent } from '../dung-chung/dieu-chuyen-tai-san/dieu-chuyen-tai-san.component';
import { KhaiBaoSuDungTaiSanComponent } from '../dung-chung/khai-bao-su-dung-tai-san/khai-bao-su-dung-tai-san.component';
import { ThuHoiTaiSanComponent } from '../dung-chung/thu-hoi-tai-san/thu-hoi-tai-san.component';
import { CreateOrEditTaiSanComponent } from '../toan-bo-tai-san/create-or-edit-tai-san/create-or-edit-tai-san.component';
import { PermissionTreeEditModel } from '@shared/dft/components/permission-tree-edit.model';

@Component({
  selector: 'app-tai-san-chua-su-dung',
  templateUrl: './tai-san-chua-su-dung.component.html',
  styleUrls: ['./tai-san-chua-su-dung.component.scss'],
  animations: [appModuleAnimation()],
})
export class TaiSanChuaSuDungComponent extends AppComponentBase implements OnInit, OnChanges {

  @Input() isActive = 0;
  @ViewChild('dt') table: Table;
  loading = true;
  exporting = false;
  keyword = '';
  totalCount = 0;
  records: TaiSanChuaSuDungForViewDto[] = [];
  arrTaiSanChecked: TaiSanChuaSuDungForViewDto[] = [];
  input: NhaCungCapGetAllInputDto;
  toChucItems: PermissionTreeEditModel;
  loaiTaiSanItems: TreeviewItem[];
  nhaCungCaps: LookupTableDto[];
  maSuDungs: LookupTableDto[];
  toChucValue: number[];
  loaiTaiSanValue: number;
  nhaCungCapId: number;
  maSuDungId: number;
  listToChucId: number[];

  form: FormGroup;
  constructor(
    injector: Injector,
    private _taiSanChuaSuDungServiceProxy: TaiSanChuaSuDungServiceProxy,
    private _fb: FormBuilder,
    private _lookupTableService: LookupTableServiceProxy,
    private _modalService: BsModalService
  ) { super(injector); }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.isActive?.currentValue === 1) {
      this.getDataPage(false);
      this.arrTaiSanChecked = [];
    }
  }

  ngOnInit(): void {

    this._lookupTableService.getAllToChucTheoNguoiDung().subscribe((data) => {
      this.toChucItems = {
        data,
        selectedData: data.map(e => e.id),
      };
      this.toChucValue = data.map(e => e.id);
      this.getDataPage(false);
    });


    this.khoiTaoForm();
    this._lookupTableService.getAllToChucIdTheoNguoiDungList(true).subscribe(rs =>
      this.listToChucId = rs
    );
    if (!this.isGranted('Pages.QuanLyTaiSanTong')) {
      forkJoin(
        this._lookupTableService.getAllToChucTheoNguoiDungTree(true),
        this._lookupTableService.getAllLoaiTaiSanTree(),
        this._lookupTableService.getAllNhaCungCap(),
        this._lookupTableService.getAllTinhTrangMaSuDungTaiSan(),
      ).subscribe(([toChuc, loaiTaiSan, nhaCungCap, tinhTrangMaSuDung]) => {
        this.loaiTaiSanItems = this.getTreeviewItem(loaiTaiSan);
        this.nhaCungCaps = nhaCungCap;
        this.maSuDungs = tinhTrangMaSuDung;
      });
    } else {
      forkJoin(
        this._lookupTableService.getAllToChucTree(),
        this._lookupTableService.getAllLoaiTaiSanTree(),
        this._lookupTableService.getAllNhaCungCap(),
        this._lookupTableService.getAllTinhTrangMaSuDungTaiSan(),
      ).subscribe(([toChucs, loaiTaiSans, nhaCungCaps, tinhTrangMaSuDungs]) => {
        this.loaiTaiSanItems = this.getTreeviewItem(loaiTaiSans);
        this.nhaCungCaps = nhaCungCaps;
        this.maSuDungs = tinhTrangMaSuDungs;
      });
    }
  }

  checkVisibleCapPhat(): boolean {
    return this.arrTaiSanChecked.filter(e => e.capPhat).length === 0;
  }
  checkVisibleDieuChuyen(): boolean {
    return this.arrTaiSanChecked.filter(e => e.dieuChuyen).length === 0;
  }
  checkVisibleThuHoi(): boolean {
    return this.arrTaiSanChecked.filter(e => e.thuHoi).length === 0;
  }

  khoiTaoForm() {
    this.form = this._fb.group({
      NccDropdownMultiple: [''],
      NhaCungCap: [''],
      MaSuDung: [''],
      DropDownTreeViewToChuc: [''],
      DropDownTreeViewLoaiTaiSan: [''],
    });
  }


  getDataPage(isSearch: boolean, lazyLoad?: LazyLoadEvent) {
    this.loading = true;
    this._taiSanChuaSuDungServiceProxy.getAll(
      this.keyword || undefined,
      this.toChucValue || undefined,
      this.loaiTaiSanValue || undefined,
      this.form.controls.NhaCungCap.value?.id,
      this.form.controls.MaSuDung.value?.id,
      isSearch,
      this.getSortField(this.table),
      lazyLoad ? lazyLoad.first : this.table.first,
      lazyLoad ? lazyLoad.rows : this.table.rows,
    ).pipe(finalize(() => { this.loading = false; }))
      .subscribe(result => {
        this.records = result.items;
        if (this.toChucValue === undefined && !this.permission.isGranted('Pages.QuanLyTaiSanTong')) {
          this.records = this.records.filter(f => this.listToChucId.includes(f.toChucId));
          this.totalCount = result.totalCount;
        } else {
          this.totalCount = result.totalCount;
        }
      });
  }

  searchTrangThaiMaSuDung(event) {
    const query = event.query;
    this._lookupTableService.getAllTinhTrangMaSuDungTaiSan().subscribe(result => {
      this.maSuDungs = this.filterTrangThai(query, result);
    });
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

  private _showCreateOrEditDemoDialog(flag: number, taiSans: TaiSanChuaSuDungForViewDto[], action: number, count: number): void {
    let createOrEditUserDialog: BsModalRef;
    if (action === TrangThaiTaiSanConst.CapPhat) {
      createOrEditUserDialog = this._modalService.show(
        CapPhatTaiSanComponent,
        {
          class: 'modal-xl',
          ignoreBackdropClick: true,
          initialState: {
            taiSans,
            flag,
            count
          },
        }
      );
    }
    if (action === TrangThaiTaiSanConst.DieuChuyen) {
      createOrEditUserDialog = this._modalService.show(
        DieuChuyenTaiSanComponent,
        {
          class: 'modal-xl',
          ignoreBackdropClick: true,
          initialState: {
            taiSans,
            flag,
            count
          },
        }
      );
    }

    if (action === TrangThaiTaiSanConst.ThuHoi) {
      createOrEditUserDialog = this._modalService.show(
        ThuHoiTaiSanComponent,
        {
          class: 'modal-xl',
          ignoreBackdropClick: true,
          initialState: {
            taiSans,
            flag,
            count
          },
        }
      );
    }

    if (action === TrangThaiTaiSanConst.TaiSanDangSuDung) {
      createOrEditUserDialog = this._modalService.show(
        KhaiBaoSuDungTaiSanComponent,
        {
          class: 'modal-xl',
          ignoreBackdropClick: true,
          initialState: {
            taiSans,
            flag,
            count
          },
        }
      );
    }
    createOrEditUserDialog.content.onSave.subscribe(() => {
      this.getDataPage(false);
      this.arrTaiSanChecked = [];
    });
  }

  capPhat(record?: TaiSanChuaSuDungForViewDto): void {
    this.pushTaiSan(record);
    this._showCreateOrEditDemoDialog(TrangThaiTabConst.TaiSanChuaSuDung, this.arrTaiSanChecked, TrangThaiTaiSanConst.CapPhat, 0);
  }

  capPhatMultiple(): void {
    const count = this.removeTaiSanChecked(TrangThaiTaiSanConst.CapPhat);
    this._showCreateOrEditDemoDialog(TrangThaiTabConst.TaiSanChuaSuDung, this.arrTaiSanChecked, TrangThaiTaiSanConst.CapPhat, count);
  }

  dieuChuyen(record?: TaiSanChuaSuDungForViewDto): void {
    this.pushTaiSan(record);
    this._showCreateOrEditDemoDialog(TrangThaiTabConst.TaiSanChuaSuDung, this.arrTaiSanChecked, TrangThaiTaiSanConst.DieuChuyen, 0);
  }

  dieuChuyenMultiple(): void {
    const count = this.removeTaiSanChecked(TrangThaiTaiSanConst.DieuChuyen);
    this._showCreateOrEditDemoDialog(TrangThaiTabConst.TaiSanChuaSuDung, this.arrTaiSanChecked, TrangThaiTaiSanConst.DieuChuyen, count);
  }

  removeTaiSanChecked(phanLoaiId: number): number {
    let count = 0;
    if (phanLoaiId === TrangThaiTaiSanConst.CapPhat) {
      this.arrTaiSanChecked.forEach(item => {
        if (!item.capPhat) {
          count++;
          this.arrTaiSanChecked = this.arrTaiSanChecked.filter(obj => obj !== item);
        }
      });
    }

    if (phanLoaiId === TrangThaiTaiSanConst.DieuChuyen) {
      this.arrTaiSanChecked.forEach(item => {
        if (!item.dieuChuyen) {
          count++;
          this.arrTaiSanChecked = this.arrTaiSanChecked.filter(obj => obj !== item);
        }
      });
    }

    if (phanLoaiId === TrangThaiTaiSanConst.ThuHoi) {
      this.arrTaiSanChecked.forEach(item => {
        if (!item.thuHoi) {
          count++;
          this.arrTaiSanChecked = this.arrTaiSanChecked.filter(obj => obj !== item);
        }
      });
    }
    return count;
  }

  thuHoi(record?: any): void {
    this.pushTaiSan(record);
    this._showCreateOrEditDemoDialog(TrangThaiTabConst.TaiSanChuaSuDung, this.arrTaiSanChecked, TrangThaiTaiSanConst.ThuHoi, 0);
  }
  thuHoiMultiple(): void {
    const count = this.removeTaiSanChecked(TrangThaiTaiSanConst.ThuHoi);
    this._showCreateOrEditDemoDialog(TrangThaiTabConst.TaiSanChuaSuDung, this.arrTaiSanChecked, TrangThaiTaiSanConst.ThuHoi, count);
  }

  khaiBaoSuDung(record?: any): void {
    this.pushTaiSan(record);
    this._showCreateOrEditDemoDialog(TrangThaiTabConst.TaiSanChuaSuDung, this.arrTaiSanChecked, TrangThaiTaiSanConst.TaiSanDangSuDung, 0);
  }
  khaiBaoSuDungMultiple(): void {
    this._showCreateOrEditDemoDialog(TrangThaiTabConst.TaiSanChuaSuDung, this.arrTaiSanChecked, TrangThaiTaiSanConst.TaiSanDangSuDung, 0);
  }
  pushTaiSan(record?: TaiSanChuaSuDungForViewDto) {
    this.arrTaiSanChecked = [];
    this.arrTaiSanChecked.push(record);
  }

  view(id?: number) {
    this._viewTaiSan(id, true);
  }

  private _viewTaiSan(id?: number, isView = false): void {
    // copy
    let createOrEditUserDialog: BsModalRef;
    createOrEditUserDialog = this._modalService.show(
      CreateOrEditTaiSanComponent,
      {
        class: 'modal-xl',
        ignoreBackdropClick: true,
        initialState: {
          id,
          isView,
        },
      }
    );

    // ouput emit
    createOrEditUserDialog.content.onSave.subscribe(() => {
      this.getDataPage(false);
    });
  }
}
