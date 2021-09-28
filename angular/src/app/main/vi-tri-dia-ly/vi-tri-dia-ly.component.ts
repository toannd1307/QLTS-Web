import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { AppConsts } from '@shared/AppConsts';
import { ImportExcelDialogComponent } from '@shared/components/import-excel/import-excel-dialog.component';
import { FileDownloadService } from '@shared/file-download.service';
import { GetAllDtos, GetAllInPutDtos, LookupTableDto, ViTriDiaLyServiceProxy } from '@shared/service-proxies/service-proxies';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';
import { finalize } from 'rxjs/operators';
import { CreateOrEditVitriDialyComponent } from './create-or-edit-vitri-dialy/create-or-edit-vitri-dialy.component';

const URL = AppConsts.remoteServiceBaseUrl + '/api/Upload/ViTriDiaLyUpload';
@Component({
  selector: 'app-vi-tri-dia-ly',
  templateUrl: './vi-tri-dia-ly.component.html',
  styleUrls: ['./vi-tri-dia-ly.component.scss'],
  animations: [appModuleAnimation()],

})
export class ViTriDiaLyComponent extends AppComponentBase implements OnInit {
  @ViewChild('dt') table: Table;
  loading = true;
  exporting = false;
  keyword = '';
  totalCount = 0;
  form: FormGroup;
  records: GetAllDtos[] = [];
  tt: LookupTableDto[];
  ttValue: number;
  qh: LookupTableDto[];
  qhValue: number;
  input: GetAllInPutDtos;

  titleNotice = 'Bạn chắc chắn không?';

  constructor(injector: Injector,
    private _modalService: BsModalService,
    private _viTriDiaLyServiceProxy: ViTriDiaLyServiceProxy,
    private _fileDownloadService: FileDownloadService,
    private _fb: FormBuilder,
    private _http: HttpClient,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.khoiTaoForm();
    this._viTriDiaLyServiceProxy.getAllDtoTinhThanh().subscribe((result) => {
      this.tt = result;
    });
  }

  khoiTaoForm() {
    this.form = this._fb.group({
      keyword: [''],
      tinhThanh: [''],
      quanHuyen: [''],
    });
  }

  getDataPage(isSearch: boolean, lazyLoad?: LazyLoadEvent) {
    this.loading = true;
    this._viTriDiaLyServiceProxy.getAll(
      this.form.controls.keyword.value || undefined,
      this.form.controls.tinhThanh.value?.id || undefined,
      this.form.controls.quanHuyen.value?.id || undefined,
      isSearch,
      this.getSortField(this.table),
      lazyLoad ? lazyLoad.first : this.table.first,
      lazyLoad ? lazyLoad.rows : this.table.rows,
    ).pipe(finalize(() => { this.loading = false; }))
      .subscribe(result => {
        this.records = result.items;
        console.log(104, result.items);
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
      CreateOrEditVitriDialyComponent,
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

  getlistQH() {
    this._viTriDiaLyServiceProxy.getAllDtoQuanHuyenFromTT(this.form.controls.tinhThanh.value?.id).subscribe((result) => {
      this.qh = result;
    });
  }

  exportToExcel() {
    this.exporting = true;
    this.input = new GetAllInPutDtos();
    this.input.fillter = this.form.controls.keyword.value || undefined;
    this.input.tinhThanh = this.form.controls.tinhThanh.value?.id || undefined;
    this.input.quanHuyen = this.form.controls.quanHuyen.value?.id || undefined;
    this.input.sorting = this.getSortField(this.table);
    this.input.skipCount = 0;
    this.input.maxResultCount = 10000000;
    this._viTriDiaLyServiceProxy.exportToExcel(this.input).subscribe((result) => {
      this._fileDownloadService.downloadTempFile(result);
      this.exporting = false;
    }, () => {
      this.exporting = false;
    });
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
      this._viTriDiaLyServiceProxy.downloadFileMau().subscribe(result => {
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
        this._viTriDiaLyServiceProxy.importFileExcel(res['result'][0]).subscribe((message) => {
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

  delete(record: GetAllDtos) {
    const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
    '<p class="text-popup-xoa m-t-8">'
     + 'Vị trí địa lý ' + record.tenViTri + ' sẽ bị xóa!' + '</p>';
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
        this._viTriDiaLyServiceProxy.deleted(record.id).subscribe(
          (result1) => {
            if (result1 === 1) {
              this.showDeleteMessage();
              this.getDataPage(false);
            } else if (result1 === 2) {
              this.showErrorMessage('Xóa thất bại!');
            } else {
              this.showWarningMessage('Vị trí đang được khai báo cho đơn vị, không được xóa!');
            }
          });
      }
    });
  }
}
