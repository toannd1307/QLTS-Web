import { HttpClient } from '@angular/common/http';
import { Component, Injector, OnChanges, OnInit, SimpleChanges, ViewChild } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { AppConsts } from '@shared/AppConsts';
import { ImportExcelDialogComponent } from '@shared/components/import-excel/import-excel-dialog.component';
import { CommonComponent } from '@shared/dft/components/common.component';
import { StatusStrPipe } from '@shared/dft/pipes/statusstr.pipe';
import { FileDownloadService } from '@shared/file-download.service';
import { KiemKeTaiSanForViewDto, KiemKeTaiSanServiceProxy, LookupTableDto, LookupTableServiceProxy } from '@shared/service-proxies/service-proxies';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { CreateOrEditKiemKeTaiSanComponent } from './create-or-edit-kiem-ke-tai-san/create-or-edit-kiem-ke-tai-san.component';
import { PermissionTreeEditModel } from '@shared/dft/components/permission-tree-edit.model';

const URL = AppConsts.remoteServiceBaseUrl + '/api/Upload/KiemKeTaiSanUpload';
@Component({
  selector: 'app-kiem-ke-tai-san',
  templateUrl: './kiem-ke-tai-san.component.html',
  styleUrls: ['./kiem-ke-tai-san.component.scss'],
  animations: [appModuleAnimation()]
})
export class KiemKeTaiSanComponent extends AppComponentBase implements OnInit {

  @ViewChild('dt') table: Table;
  loading = true;
  exporting = false;
  keyword = '';
  totalCount = 0;
  records: KiemKeTaiSanForViewDto[] = [];
  input: {};
  toChucItems: PermissionTreeEditModel;
  toChucValue: number[];
  suggestions: LookupTableDto[];
  arrTrangThaiKiemKe: any[];
  checkedAll: true;
  selectedRows: KiemKeTaiSanForViewDto[] = [];
  rangeDates: any[] = [];
  trangthai: LookupTableDto;
  trangthaiKiemKe: LookupTableDto[];

  constructor(
    injector: Injector,
    private _modalService: BsModalService,
    private _kiemKeTaiSanService: KiemKeTaiSanServiceProxy,
    private _lookupTableService: LookupTableServiceProxy,
  ) { super(injector); }


  ngOnInit(): void {
    this.rangeDates[0] = CommonComponent.getNgayDauTienCuaThangHienTaiDate();
    this.rangeDates[1] = new Date();
    // forkJoin(
    //   this._lookupTableService.getAllToChucTheoNguoiDung(),
    //   this._lookupTableService.getAllTrangThaiKiemKe(),
    // ).subscribe(([toChuc, trangThaiKiemKe]) => {
    //   this.toChucItems = new PermissionTreeEditModel {
    //     toChuc,
    //     selectedData: toChuc.map(e => e.id),
    //   };
    //   this.toChucValue = toChuc.map(e => e.id);
    //   this.trangthaiKiemKe = trangThaiKiemKe;
    //   this.getDataPage();
    // });
    this._lookupTableService.getAllToChucTheoNguoiDung().subscribe((data) => {
      this.toChucItems = {
        data,
        selectedData: data.map(e => e.id),
      };
      this.toChucValue = data.map(e => e.id);
      this.getDataPage(false);
    });
    this._lookupTableService.getAllTrangThaiKiemKe().subscribe((trangThaiKiemKe) => {
      this.trangthaiKiemKe = trangThaiKiemKe;
    });
  }

  getDataPage(isSearch: boolean, lazyLoad?: LazyLoadEvent) {
    this.selectedRows = [];
    this.loading = true;
    const filterPipe = new StatusStrPipe();
    const trangthaiID = this.trangthai ? this.trangthai.displayName : null;
    if (this.toChucValue !== undefined) {
      this._kiemKeTaiSanService.getAll(
        this.keyword || undefined,
        this.rangeDates ? this.rangeDates[0] : undefined,
        this.rangeDates ? this.rangeDates[1] : undefined,
        this.toChucValue,
        filterPipe.transform(trangthaiID),
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
  }

  selectRow(checkValue) {
    if (checkValue) {
      this.selectedRows = this.records.filter(value => value);
    } else {
      this.selectedRows = [];
    }
  }
  searchTrangThaiKiemKe(event) {
    const query = event.query;
    this._lookupTableService.getAllTrangThaiKiemKe().subscribe(result => {
      this.suggestions = this.filterTrangThaiKiemKe(query, result);
    });
  }
  filterTrangThaiKiemKe(query, trangThai: LookupTableDto[]): any[] {
    const filtered: any[] = [];
    for (const iterator of trangThai) {
      if (iterator.displayName.toLowerCase().indexOf(query.toLowerCase()) === 0) {
        filtered.push(iterator);
      }
    }
    return filtered;
  }

  create(id?: number) {
    this._showCreateOrEditDialog(id, false);
  }

  view(id?: number, trangThaiID?: number) {
    this._showCreateOrEditDialog(id, true, trangThaiID);
  }

  delete(record) {
    const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
    '<p class="text-popup-xoa m-t-8">'
     + 'Đợt kiểm kê ' + record.kiemKeTaiSan.tenKiemKe + ' sẽ bị xóa!' + '</p>';
    this.swal.fire({
      html: html1,
      icon: 'warning',
      iconHtml: '<span class="icon1">&#9888</span>',
      showCancelButton: true,
      confirmButtonColor: this.confirmButtonColor,
      cancelButtonColor: this.cancelButtonColor,
      cancelButtonText: this.cancelButtonText,
      confirmButtonText: this.confirmButtonText
    }).then((result) => {
      if (result.value) {
        this._kiemKeTaiSanService.delete([record.kiemKeTaiSan.id]).subscribe(() => {
          this.showDeleteMessage();
          this.getDataPage(false);
        });
      }
    });
    // }
  }
  isDeletes() {
    if (this.selectedRows.length === 0) {
      return true;
    }
    return false;
  }

  deletes() {
    const input = [];
    let isTrangThai = false; // check trạng thái kiểm kê khác đang kiểm kê và đang hoạt động
    this.selectedRows.forEach(e => {
      if (e.kiemKeTaiSan.trangThaiId === 1 || e.kiemKeTaiSan.trangThaiId === 2) {
        isTrangThai = true;
      }
      input.push(e.kiemKeTaiSan.id);
    });
    if (isTrangThai) {
      if (this.selectedRows.length === 1) {
        const whatStatus = this.selectedRows[0].kiemKeTaiSan.trangThaiId === 1 ? '"Đang kiểm kê"' : '"Đã kết thúc"';
        this.showExistMessage('Đợt kiểm kê ' + this.selectedRows[0].kiemKeTaiSan.tenKiemKe + ' đã chọn đang ở trạng thái ' + whatStatus + ' không được phép xoá.');
      } else { this.showExistMessage('Một hay nhiều đợt kiểm kê đã chọn đang ở trạng thái "Đang kiểm kê" hoặc "Đã kết thúc" không được phép xoá.'); }
    } else {
      const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
      '<p class="text-popup-xoa m-t-8">'
       + 'Các đợt kiểm kê đã chọn sẽ bị xóa!' + '</p>';
      this.swal.fire({
        html: html1,
        icon: 'warning',
        iconHtml: '<span class="icon1">&#9888</span>',
        showCancelButton: true,
        confirmButtonColor: this.confirmButtonColor,
        cancelButtonColor: this.cancelButtonColor,
        cancelButtonText: this.cancelButtonText,
        confirmButtonText: this.confirmButtonText
      }).then((result) => {
        if (result.value) {
          this._kiemKeTaiSanService.delete(input).subscribe(() => {
            this.showDeleteMessage();
            this.getDataPage(false);
          });
        }
      });
    }

  }
  // show modal popup
  private _showCreateOrEditDialog(id?: number, isView = false, trangThaiID?: number): void {
    // copy
    let createOrEditUserDialog: BsModalRef;
    createOrEditUserDialog = this._modalService.show(
      CreateOrEditKiemKeTaiSanComponent,
      {
        class: 'modal-xl',
        ignoreBackdropClick: true,
        initialState: {
          id,
          isView,
          trangThaiID
        },
      }
    );
    // ouput emit
    createOrEditUserDialog.content.onSave.subscribe(() => {
      this.getDataPage(false);
    });
  }

}
