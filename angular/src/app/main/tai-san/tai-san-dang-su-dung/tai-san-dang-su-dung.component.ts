import { AppComponentBase } from '@shared/app-component-base';
import { Component, Injector, Input, OnChanges, OnInit, SimpleChanges, ViewChild } from '@angular/core';
import { Table } from 'primeng/table';
import { LookupTableDto, LookupTableServiceProxy, NhaCungCapGetAllInputDto, TaiSanChuaSuDungForViewDto, TaiSanDangSuDungServiceProxy } from '@shared/service-proxies/service-proxies';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { FormBuilder, FormGroup } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { forkJoin } from 'rxjs';
import { LazyLoadEvent } from 'primeng/api';
import { finalize } from 'rxjs/operators';
import { TrangThaiTabConst, TrangThaiTaiSanConst } from '@shared/AppEnums';
import { CapPhatTaiSanComponent } from '../dung-chung/cap-phat-tai-san/cap-phat-tai-san.component';
import { DieuChuyenTaiSanComponent } from '../dung-chung/dieu-chuyen-tai-san/dieu-chuyen-tai-san.component';
import { ThuHoiTaiSanComponent } from '../dung-chung/thu-hoi-tai-san/thu-hoi-tai-san.component';
import { KhaiBaoSuDungTaiSanComponent } from '../dung-chung/khai-bao-su-dung-tai-san/khai-bao-su-dung-tai-san.component';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { CreateOrEditTaiSanComponent } from '../toan-bo-tai-san/create-or-edit-tai-san/create-or-edit-tai-san.component';
import { PermissionTreeEditModel } from '@shared/dft/components/permission-tree-edit.model';

@Component({
  selector: 'app-tai-san-dang-su-dung',
  templateUrl: './tai-san-dang-su-dung.component.html',
  styleUrls: ['./tai-san-dang-su-dung.component.scss'],
  animations: [appModuleAnimation()],
})
export class TaiSanDangSuDungComponent extends AppComponentBase implements OnInit, OnChanges {

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
  tenTaiSan = '';
  checkTaiSan24h = this.showUndoButton;

  form: FormGroup;
  constructor(
    injector: Injector,
    private _taiSanDangSuDungServiceProxy: TaiSanDangSuDungServiceProxy,
    private _fb: FormBuilder,
    private _lookupTableService: LookupTableServiceProxy,
    private _modalService: BsModalService
  ) { super(injector); }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.isActive?.currentValue === 2) {
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

    this.khoiTaoForm();

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
      NccDropdownMultiple: [],
      NhaCungCap: [],
      MaSuDung: [],
      DropDownTreeViewToChuc: [],
      DropDownTreeViewLoaiTaiSan: [],
    });
  }


  getDataPage(isSearch: boolean, lazyLoad?: LazyLoadEvent) {
    this.loading = true;
    this._taiSanDangSuDungServiceProxy.getAll(
      this.keyword || undefined,
      this.toChucValue || undefined,
      this.loaiTaiSanValue || undefined,
      this.form.controls.NhaCungCap.value?.id || undefined,
      this.form.controls.MaSuDung.value?.id || undefined,
      isSearch,
      this.getSortField(this.table),
      lazyLoad ? lazyLoad.first : this.table.first,
      lazyLoad ? lazyLoad.rows : this.table.rows,
    ).pipe(finalize(() => { this.loading = false; }))
      .subscribe(result => {
        this.totalCount = result.totalCount;
        this.records = result.items;
        if (this.toChucValue === undefined && !this.permission.isGranted('Pages.QuanLyTaiSanTong')) {
          this.records = this.records.filter(f => this.listToChucId.includes(f.toChucId));
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
    this._showCreateOrEditDemoDialog(TrangThaiTabConst.TaiSanDangSuDung, this.arrTaiSanChecked, TrangThaiTaiSanConst.CapPhat, 0);
  }

  capPhatMultiple(): void {
    const count = this.removeTaiSanChecked(TrangThaiTaiSanConst.CapPhat);
    this._showCreateOrEditDemoDialog(TrangThaiTabConst.TaiSanDangSuDung, this.arrTaiSanChecked, TrangThaiTaiSanConst.CapPhat, count);
  }

  dieuChuyen(record?: TaiSanChuaSuDungForViewDto): void {
    this.pushTaiSan(record);
    this._showCreateOrEditDemoDialog(TrangThaiTabConst.TaiSanDangSuDung, this.arrTaiSanChecked, TrangThaiTaiSanConst.DieuChuyen, 0);
  }

  dieuChuyenMultiple(): void {
    const count = this.removeTaiSanChecked(TrangThaiTaiSanConst.DieuChuyen);
    this._showCreateOrEditDemoDialog(TrangThaiTabConst.TaiSanDangSuDung, this.arrTaiSanChecked, TrangThaiTaiSanConst.DieuChuyen, count);
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
    this._showCreateOrEditDemoDialog(TrangThaiTabConst.TaiSanDangSuDung, this.arrTaiSanChecked, TrangThaiTaiSanConst.ThuHoi, 0);
  }
  thuHoiMultiple(): void {
    const count = this.removeTaiSanChecked(TrangThaiTaiSanConst.ThuHoi);
    this._showCreateOrEditDemoDialog(TrangThaiTabConst.TaiSanDangSuDung, this.arrTaiSanChecked, TrangThaiTaiSanConst.ThuHoi, count);
  }

  deleteArrTaiSanChecked(): void {
    this.tenTaiSan = this.arrTaiSanChecked.filter(f => this.checkTaiSan24h(f.ngayKhaiBao)).map(m => { return m.tenTaiSan; }).join(', ');
    const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
    '<p class="text-popup-xoa m-t-8">'
     + 'Danh sách tài sản đã chọn sẽ được hoàn tác về danh sách trước khi khai báo sử dụng' + '</p>';
    this.swal.fire({
      html: html1,
      icon: 'warning',
      iconHtml: '<span class="icon1">&#9888</span>',
      showCancelButton: true,
      confirmButtonColor: this.confirmButtonColor,
      cancelButtonColor: this.cancelButtonColor,
      cancelButtonText: this.cancelButtonText,
      confirmButtonText: 'Hoàn tác'
    }).then((result) => {
      if (result.value) {
        this.clearArrChecked(this.arrTaiSanChecked.filter(f => this.checkTaiSan24h(f.ngayKhaiBao)));
      }
    });
  }

  checkHoanTac(record?: TaiSanChuaSuDungForViewDto): boolean {
    if (this.checkTaiSan24h(record.ngayKhaiBao)) {
      return true;
    }
    return false;
  }

  hoanTac(record?: TaiSanChuaSuDungForViewDto): void {
    this.tenTaiSan = this.checkTaiSan24h(record.ngayKhaiBao) === true ? record.tenTaiSan : '';
    const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
    '<p class="text-popup-xoa m-t-8">'
     + 'Tài sản ' + this.tenTaiSan + ' sẽ hoàn tác về ' + 'danh sách trước khi khai báo sử dụng' + '</p>';
    this.swal.fire({
      html: html1,
      icon: 'warning',
      iconHtml: '<span class="icon1">&#9888</span>',
      showCancelButton: true,
      confirmButtonColor: this.confirmButtonColor,
      cancelButtonColor: this.cancelButtonColor,
      cancelButtonText: this.cancelButtonText,
      confirmButtonText: 'Hoàn tác'
    }).then((result) => {
      if (result.value) {
        this.clearArrChecked(this.arrTaiSanChecked.filter(f => this.checkTaiSan24h(f.ngayKhaiBao)));
        this._lookupTableService.deleteTaiSan(record.id).subscribe(() => {
          this.showSuccessMessage('Hoàn tác thành công!');
          this.getDataPage(false);
        });
      }
    });
  }

  checkVisibleHoanTac(): boolean {
    return this.arrTaiSanChecked.filter(e => this.checkTaiSan24h(e.ngayKhaiBao)).length === 0;
  }

  clearArrChecked(arrTaiSanChecked) {
    this._taiSanDangSuDungServiceProxy.hoanTacTaiSanList(arrTaiSanChecked.map(m => m.id)).subscribe(() => {
      this.arrTaiSanChecked = [];
      this.showSuccessMessage('Hoàn tác thành công!');
      this.getDataPage(false);
    });
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
