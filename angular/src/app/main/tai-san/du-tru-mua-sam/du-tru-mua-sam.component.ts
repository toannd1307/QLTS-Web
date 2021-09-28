import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { FileDownloadService } from '@shared/file-download.service';
import {
  DuTruMuaSamInput, DuTruMuaSamOutPut, LookupTableDto, LookupTableServiceProxy,
  MailServerServiceProxy, PhieuDuTruMuaSamServiceProxy,
} from '@shared/service-proxies/service-proxies';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';
import { finalize } from 'rxjs/operators';
import { PermissionTreeEditModel } from '@shared/dft/components/permission-tree-edit.model';
import { CreateOrEditDuTruMuaSamComponent } from './create-or-edit-du-tru-mua-sam/create-or-edit-du-tru-mua-sam.component';
@Component({
  selector: 'app-du-tru-mua-sam',
  templateUrl: './du-tru-mua-sam.component.html',
  styleUrls: ['./du-tru-mua-sam.component.scss'],
  animations: [appModuleAnimation()],
})
export class DuTruMuaSamComponent extends AppComponentBase implements OnInit {

  @ViewChild('dt') table: Table;
  loading = true;
  exporting = false;
  totalCount = 0;
  records: DuTruMuaSamOutPut[] = [];
  input: DuTruMuaSamInput;
  nhaCC: LookupTableDto[];
  maSD: LookupTableDto[];
  ttSD: LookupTableDto[];
  form: FormGroup;
  selectedList: DuTruMuaSamOutPut[] = [];
  loaiTaiSanItems: TreeviewItem[];
  loaiTaiSanValue: number;

  toChucItems: PermissionTreeEditModel;
  toChucValue: number[];
  listIdXoa: number[] = [];
  titleNotice = 'Bạn chắc chắn không?';
  text = 'Phiếu dự trù mua sắm ';
  listTrangThaiTaiSan: LookupTableDto[];
  keyword = '';
  constructor(injector: Injector,
    private _modalService: BsModalService,
    private _duTruMuaSamServiceProxy: PhieuDuTruMuaSamServiceProxy,
    private _fb: FormBuilder,
    private _fileDownloadService: FileDownloadService,
    private _lookupTableService: LookupTableServiceProxy,
    private _sendMail: MailServerServiceProxy,
  ) { super(injector); }

  ngOnInit(): void {

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

    this._lookupTableService.getAllTrangThaiTaiSanTimKiem().subscribe((w) => {
      this.listTrangThaiTaiSan = w;
    });
    this.khoiTaoForm();
  }

  khoiTaoForm() {
    this.form = this._fb.group({
      keyword: [''],
      PhongBan: [''],
      TinhTrangFilter: [''],
    });
  }

  getDataPage(isSearch: boolean, lazyLoad?: LazyLoadEvent) {
    this.selectedList = [];
    this.loading = true;
    this._duTruMuaSamServiceProxy.getAll(
      this.form.controls.keyword.value || undefined,
      this.toChucValue,
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

  view(id?: number) {
    this._showCreateOrEditDemoDialog(id, true);
  }

  private _showCreateOrEditDemoDialog(id?: number, isView = false): void {
    // copy
    let createOrEditUserDialog: BsModalRef;
    createOrEditUserDialog = this._modalService.show(
      CreateOrEditDuTruMuaSamComponent,
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

  delete(record: DuTruMuaSamOutPut) {
    if (record.trangThaiId === 0) {
      const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
      '<p class="text-popup-xoa m-t-8">'
       + this.text + record.tenPhieu + ' sẽ bị xóa!' + '</p>';
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
          this._duTruMuaSamServiceProxy.deleted(record.id).subscribe(() => {
            this.showDeleteMessage();
            this.getDataPage(false);
          });
        }
      });
    }
  }

  hoanThanh(record: DuTruMuaSamOutPut) {
    if (record.trangThaiId === 0) {
      const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
      '<p class="text-popup-xoa m-t-8">'
       + this.text + record.tenPhieu + ' thay đổi trạng thái Hoàn thành!' + '</p>';
      this.swal.fire({
        html: html1,
        icon: 'warning',
        iconHtml: '<span class="icon1">&#9888</span>',
        showCancelButton: true,
        confirmButtonColor: this.confirmButtonColor,
        cancelButtonColor: this.cancelButtonColor,
        cancelButtonText: this.cancelButtonText,
        confirmButtonText: 'Hoàn thành'
      }).then((result) => {
        if (result.value) {
          this._duTruMuaSamServiceProxy.hoanThanh(record.id).subscribe(() => {
            this._sendMail.sendMailHoanThanhPhieu(record).subscribe(rs => { });
            this.showUpdateMessage();
            this.getDataPage(false);
          });
        }
      });
    }
  }

  huyBo(record: DuTruMuaSamOutPut) {
    if (record.trangThaiId === 0) {
      const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
      '<p class="text-popup-xoa m-t-8">'
       + this.text + record.tenPhieu + ' thay đổi trạng thái Hủy bỏ!' + '</p>';
      this.swal.fire({
        html: html1,
        icon: 'warning',
        iconHtml: '<span class="icon1">&#9888</span>',
        showCancelButton: true,
        confirmButtonColor: this.confirmButtonColor,
        cancelButtonColor: this.cancelButtonColor,
        cancelButtonText: this.cancelButtonText,
        confirmButtonText: 'Xác nhận'
      }).then((result) => {
        if (result.value) {
          this._duTruMuaSamServiceProxy.huyBo(record.id).subscribe(() => {
            this._sendMail.sendMailHuyPhieu(record).subscribe(rs => { });
            this.showUpdateMessage();
            this.getDataPage(false);
          });
        }
      });
    }
  }

  xoaList(res: DuTruMuaSamOutPut[]) {

    const listResult = res.filter(f => f.trangThaiId === +0);
    listResult.forEach(item => {
      this.listIdXoa.push(item.id);
    });
    if (this.listIdXoa.length > 0) {
      const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
      '<p class="text-popup-xoa m-t-8">'
       + 'Danh sách phiếu dự trù mua sắm đã chọn sẽ bị xóa.' + '</p>';
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
          this._duTruMuaSamServiceProxy.xoaList(this.listIdXoa).subscribe(() => {
            this.showDeleteMessage();
            this.getDataPage(false);
          });
        }
      });
    }
  }

  close() {
    // tslint:disable-next-line:prefer-const
    let a: BsModalRef;
    a.hide();
  }

  exportToExcel() {
    this.exporting = true;
    this.input = new DuTruMuaSamInput();
    this.input.keyword = this.form.controls.keyword.value || undefined,
      this.input.skipCount = 0;
    this.input.maxResultCount = 10000000;
    this._duTruMuaSamServiceProxy.exportToExcel(this.input).subscribe((result) => {
      this._fileDownloadService.downloadTempFile(result);
      this.exporting = false;
    }, () => {
      this.exporting = false;
    });
  }
}
