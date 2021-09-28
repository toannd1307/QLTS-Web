import { Component, OnInit, Injector, ViewChild, OnDestroy, Input, OnChanges, SimpleChanges } from '@angular/core';
import { Table } from 'primeng/table';
import { AppComponentBase } from '@shared/app-component-base';
import {
  LookupTableDto,
  LookupTableServiceProxy,
  MailServerServiceProxy,
  TaiSanSuaChuaBaoDuongServiceProxy,
  ViewTaiSanSuaChuaBaoDuong,
} from '@shared/service-proxies/service-proxies';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { LazyLoadEvent } from 'primeng/api';
import { finalize, map } from 'rxjs/operators';
import { FormGroup, FormBuilder } from '@angular/forms';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { CreateOrEditTaiSanSuaChuaBaoDuongComponent } from './create-or-edit-tai-san-sua-chua-bao-duong/create-or-edit-tai-san-sua-chua-bao-duong.component';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { forkJoin, Subscription } from 'rxjs';
import { CreateOrEditTaiSanComponent } from '../toan-bo-tai-san/create-or-edit-tai-san/create-or-edit-tai-san.component';
import { PermissionTreeEditModel } from '@shared/dft/components/permission-tree-edit.model';

@Component({
  selector: 'app-tai-san-sua-chua-bao-duong',
  templateUrl: './tai-san-sua-chua-bao-duong.component.html',
  styleUrls: ['./tai-san-sua-chua-bao-duong.component.scss'],
  animations: [appModuleAnimation()],
})
export class TaiSanSuaChuaBaoDuongComponent extends AppComponentBase implements OnInit, OnDestroy, OnChanges {

  @Input() isActive = 0;
  @ViewChild('dt') table: Table;
  loading = true;
  exporting = false;
  totalCount = 0;
  records: ViewTaiSanSuaChuaBaoDuong[] = [];
  // input: TaiSanGetAllInputDto;
  suggestionsSingle: LookupTableDto[];
  form: FormGroup;
  toChucValue: number[];

  toChucItems: PermissionTreeEditModel;
  loaiTaiSanItems: TreeviewItem[];
  arrNhaCungCap: LookupTableDto[];
  arrHinhThuc: LookupTableDto[];
  arrTrangThai: LookupTableDto[];

  arrDonVi = ['', '', '', '', '', 'Sửa chữa', 'Bảo dưỡng'];

  arrTaiSanChecked: ViewTaiSanSuaChuaBaoDuong[] = [];
  selectedValues: string[] = [];

  tenTaiSan = '';

  private _unsubscribe: Subscription[] = [];

  checkTaiSan24h = this.showUndoButton;

  constructor(injector: Injector,
    private _modalService: BsModalService,
    private _taiSanServiceProxy: TaiSanSuaChuaBaoDuongServiceProxy,
    private _lookupTableService: LookupTableServiceProxy,
    private _fb: FormBuilder,
    private _sendMail: MailServerServiceProxy
  ) {
    super(injector);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.isActive?.currentValue === 3) {
      this.getDataPage(false);
      this.arrTaiSanChecked = [];
    }
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
      this._lookupTableService.getAllNhaCungCap(),
      this._lookupTableService.getAllTrangThaiTaiSan(),
      this._lookupTableService.getAllTrangThaiSuaChuaBaoDuong(),
    ).subscribe(([loaiTaiSan, nhaCungCap, trangThaiTaiSan, trangThaiSuaChuaBaoDuong]) => {
      this.loaiTaiSanItems = this.getTreeviewItem(loaiTaiSan);
      this.arrNhaCungCap = nhaCungCap;
      this.arrHinhThuc = trangThaiTaiSan.filter(ff =>
        ff.id === this.enums.TrangThaiTaiSanConst.TaiSanSuaChua ||
        ff.id === this.enums.TrangThaiTaiSanConst.TaiSanBaoDuong);
      this.arrTrangThai = trangThaiSuaChuaBaoDuong.filter(ff =>
        ff.id === this.enums.TrangThaiSuaChuaBaoDuongConst.DangThucHien ||
        ff.id === this.enums.TrangThaiSuaChuaBaoDuongConst.KhongThanhCong);
    });
  }

  ngOnDestroy(): void {
    this._unsubscribe.forEach((sb) => sb.unsubscribe());
  }

  getDataPage(isSearch: boolean, lazyLoad?: LazyLoadEvent) {
    this.arrTaiSanChecked = [];
    this.loading = true;
    this._taiSanServiceProxy.getAllTaiSanSuaChuaBaoDuong(
      this.form.value.tenTaiSan || undefined,
      this.toChucValue,
      this.form.value.loaiTaiSan || undefined,
      this.form.value.nhaCungCap?.displayName || undefined,
      this.form.value.hinhThuc?.id || undefined,
      this.form.value.trangThai?.id || undefined,
      isSearch,
      this.getSortField(this.table),
      lazyLoad ? lazyLoad.first : this.table.first,
      lazyLoad ? lazyLoad.rows : this.table.rows,
    ).pipe(finalize(() => { this.loading = false; }))
      .subscribe(result => {
        this.records = result.items;
        this.totalCount = result.totalCount;
      });
  }

  create(id?: number) {
    this._showCreateOrEditDemoDialog(id);
  }

  success(record) {
    if (record) {
      record.trangThai = this.enums.TrangThaiSuaChuaBaoDuongConst.ThanhCong;
      this._taiSanServiceProxy.editTaiSanSuaChuaBaoDuong(record).subscribe(() => {
        this._sendMail.sendMailKetQuaBaoDuong(record).subscribe(rs => { });
        this.getDataPage(false);
        this.showUpdateMessage();
      });
    }
  }

  unsuccess(record) {
    if (record) {
      record.trangThai = this.enums.TrangThaiSuaChuaBaoDuongConst.KhongThanhCong;
      this._taiSanServiceProxy.editTaiSanSuaChuaBaoDuong(record).subscribe(() => {
        this._sendMail.sendMailKetQuaBaoDuong(record).subscribe(rs1 => { });
        this.getDataPage(false);
        this.showUpdateMessage();
      });
    }
  }

  delete(record: ViewTaiSanSuaChuaBaoDuong) {
    this._lookupTableService.getTrangThaiTaiSanTruoc(record.id).subscribe(trangThaiTaiSanName => {
      const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
      '<p class="text-popup-xoa m-t-8">'
       + 'Tài sản ' + record.tenTaiSan + ' sẽ hoàn tác về "Danh sách Tài sản ' + trangThaiTaiSanName + '"' + '</p>';
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
          this.clearArrChecked([record]);
        }
      });
    });
  }

  deleteArrTaiSanChecked() {
    this.tenTaiSan = this.arrTaiSanChecked.filter(f => this.checkTaiSan24h(f.lastModificationTime)).map(m => { return m.tenTaiSan; }).join(', ');
    const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
    '<p class="text-popup-xoa m-t-8">'
     + 'Danh sách tài sản đã chọn sẽ được hoàn tác về danh sách trước khi khai báo sửa chữa/bảo dưỡng' + '</p>';
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
        this.clearArrChecked(this.arrTaiSanChecked.filter(f => this.checkTaiSan24h(f.lastModificationTime)));
      }
    });
  }

  clearArrChecked(arrTaiSanChecked) {
    this._taiSanServiceProxy.hoanTacTaiSanSuaChuaBaoDuong(arrTaiSanChecked.map(m => { return { id: m.id }; })).subscribe(() => {
      this.arrTaiSanChecked = [];
      this.showSuccessMessage('Hoàn tác thành công!');
      this.getDataPage(false);
    });
  }

  private _showCreateOrEditDemoDialog(id?: number, isView = false): void {
    // copy
    let createOrEditUserDialog: BsModalRef;
    createOrEditUserDialog = this._modalService.show(
      CreateOrEditTaiSanSuaChuaBaoDuongComponent,
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

  view(id?: number) {
    this._showCreateOrEditTaiSanDialog(id, true);
  }

  private _showCreateOrEditTaiSanDialog(id?: number, isView = false): void {
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

  search(event, item) {
    const query = event.query;
    switch (item) {
      case 'nhaCungCap':
        this._lookupTableService.getAllNhaCungCap().subscribe(result => {
          this.suggestionsSingle = this.filter(query, result);
        });
        break;
      case 'hinhThuc':
        this._lookupTableService.getAllTrangThaiTaiSan().pipe(
          map(f => f.filter(ff =>
            ff.id === this.enums.TrangThaiTaiSanConst.TaiSanSuaChua ||
            ff.id === this.enums.TrangThaiTaiSanConst.TaiSanBaoDuong)),
        ).subscribe(result => {
          this.suggestionsSingle = this.filter(query, result);
        });
        break;
      case 'trangThai':
        this._lookupTableService.getAllTrangThaiSuaChuaBaoDuong().pipe(
          map(f => f.filter(ff =>
            ff.id === this.enums.TrangThaiSuaChuaBaoDuongConst.DangThucHien ||
            ff.id === this.enums.TrangThaiSuaChuaBaoDuongConst.KhongThanhCong)),
        ).subscribe(result => {
          this.suggestionsSingle = this.filter(query, result);
        });
        break;
      default: break;
    }
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

  formCreate() {
    this.form = this._fb.group({
      tenTaiSan: [''],
      phongBanQuanLy: [''],
      loaiTaiSan: [''],
      nhaCungCap: [],
      hinhThuc: [],
      trangThai: [],
    });
  }
}
