import { HttpClient } from '@angular/common/http';
import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { AppConsts } from '@shared/AppConsts';
import { ImportExcelDialogComponent } from '@shared/components/import-excel/import-excel-dialog.component';
import { FileDownloadService } from '@shared/file-download.service';
import { AngTenRFIDServiceProxy, GetAllOutputDto, InputRFIDDto, LookupTableDto, LookupTableServiceProxy, } from '@shared/service-proxies/service-proxies';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';
import { finalize } from 'rxjs/operators';
import { CreateOrEditAngtenRfidComponent } from './create-or-edit/create-or-edit-angten-rfid/create-or-edit-angten-rfid.component';
import { PermissionTreeEditModel } from '@shared/dft/components/permission-tree-edit.model';
const URL = AppConsts.remoteServiceBaseUrl + '/api/Upload/NhaCungCapUpload';
@Component({
  selector: 'app-angten-rfid',
  templateUrl: './angten-rfid.component.html',
  styleUrls: ['./angten-rfid.component.scss'],
  animations: [appModuleAnimation()],
})
export class AngtenRfidComponent extends AppComponentBase implements OnInit {

  @ViewChild('dt') table: Table;
  loading = true;
  exporting = false;
  totalCount = 0;
  records: GetAllOutputDto[] = [];
  input: InputRFIDDto;
  nhaCC: LookupTableDto[];
  maSD: LookupTableDto[];
  ttSD: LookupTableDto[];
  form: FormGroup;
  selectedList: GetAllOutputDto[] = [];
  loaiTaiSanItems: TreeviewItem[];
  loaiTaiSanValue: number;

  toChucItems: PermissionTreeEditModel;
  toChucValue: number[];
  listIdXoa: number[] = [];
  titleNotice = 'Bạn chắc chắn không?';
  listTrangThaiTaiSan: LookupTableDto[];
  keyword = '';
  constructor(injector: Injector,
    private _modalService: BsModalService,
    private _angtenRFIDServiceProxy: AngTenRFIDServiceProxy,
    private _fb: FormBuilder,
    private _http: HttpClient,
    private _fileDownloadService: FileDownloadService,
    private _lookupTableService: LookupTableServiceProxy,
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
    console.log(lazyLoad);
    this.selectedList = [];
    this.loading = true;
    this._angtenRFIDServiceProxy.getAll(
      this.keyword || undefined,
      this.toChucValue,
      this.form.controls.TinhTrangFilter.value?.id,
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
      CreateOrEditAngtenRfidComponent,
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

  delete(record: GetAllOutputDto) {
    if (record.trangThaiId === 0) {
      const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
      '<p class="text-popup-xoa m-t-8">'
       + 'Tài sản ' + record.tenTS + ' sẽ bị xóa!' + '</p>';
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
          this._angtenRFIDServiceProxy.deleted(record.id).subscribe(() => {
            this.showDeleteMessage();
            this.getDataPage(false);
          });
        }
      });
    } else {
      this._thongBao('Tài sản ' + record.tenTS + ' sẽ bị xóa!', 'Đầu đọc cố định đang có rằng buộc dữ liệu, không được xóa!');
    }

  }

  xoaList(res: GetAllOutputDto[]) {

    const listResult = res.filter(f => f.trangThaiId === +0);
    listResult.forEach(item => {
      this.listIdXoa.push(item.id);
    });
    if (this.listIdXoa.length > 0) {
      const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
      '<p class="text-popup-xoa m-t-8">'
       + 'Danh sách tài sản đã chọn sẽ bị xóa.' + '</p>';
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
          this._angtenRFIDServiceProxy.xoaList(this.listIdXoa).subscribe(() => {
            this.showDeleteMessage();
            this.getDataPage(false);
          });
        }
      });
    } else {
      this._thongBao('Danh sách tài sản đã chọn sẽ bị xóa.', 'Xóa không thành công!');
    }
  }

  importExcel() {
    this._showImportDemoDialog();
  }

  private _showImportDemoDialog(): void {
    let importExcelDialog: BsModalRef;

    importExcelDialog = this._modalService.show(
      ImportExcelDialogComponent,
      {
        class: 'modal-lg',
        ignoreBackdropClick: true,
        initialState: {
          maxFile: 1,
          excelAcceptTypes: this.excelAcceptTypes
        }
      }
    );

    // Tải file mẫu
    importExcelDialog.content.onDownload.subscribe(() => {
      this._angtenRFIDServiceProxy.downloadFileMau().subscribe(result => {
        importExcelDialog.content.downLoading = false;
        this._fileDownloadService.downloadTempFile(result);
      });
    });

    // Upload
    importExcelDialog.content.onSave.subscribe((ouput) => {
      importExcelDialog.content.returnMessage = 'Đang upload file....';
      const formdata = new FormData();
      for (let i = 0; i < ouput.length; i++) {
        formdata.append((i + 1) + '', ouput[i]);
      }
      this._http.post(URL, formdata).subscribe((res) => {
        this._angtenRFIDServiceProxy.importFileExcel(res['result'][0]).subscribe((message) => {
          importExcelDialog.content.returnMessage = message;
          importExcelDialog.content.uploading = false;
          importExcelDialog.content.uploadDone();
        });
      });
    });

    // Close
    importExcelDialog.content.onClose.subscribe(() => {
      this.getDataPage(false);
    });
  }

  exportToExcel() {
    this.exporting = true;
    this.input = new InputRFIDDto();
    this.input.tenTS = this.keyword || undefined;
    this.input.phongBanSuDung = this.toChucValue;
    this.input.tinhTrangSuDung = this.form.controls.TinhTrangFilter.value?.id;
    this.input.skipCount = 0;
    this.input.maxResultCount = 10000000;
    this._angtenRFIDServiceProxy.exportToExcel(this.input).subscribe((result) => {
      this._fileDownloadService.downloadTempFile(result);
      this.exporting = false;
    }, () => {
      this.exporting = false;
    });
  }

  private _thongBao(textXoa: string, textExit: string) {
    const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
    '<p class="text-popup-xoa m-t-8">'
     + textXoa + '</p>';
    this.swal.fire({
      html: html1,
      icon: 'warning',
      iconHtml: '<span class="icon1">&#9888</span>',
      showCancelButton: true,
      confirmButtonColor: this.confirmButtonColor,
      cancelButtonColor: this.cancelButtonColor,
      cancelButtonText: this.cancelButtonText,
      confirmButtonText: this.confirmButtonText
    }).then(() => {
      this.showExistMessage(textExit);
    });
  }
}
