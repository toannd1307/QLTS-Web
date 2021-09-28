import { HttpClient } from '@angular/common/http';
import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { AppConsts } from '@shared/AppConsts';
import { ImportExcelDialogComponent } from '@shared/components/import-excel/import-excel-dialog.component';
import { FileDownloadService } from '@shared/file-download.service';
import { LookupTableDto, LookupTableServiceProxy, NhaCungCapForViewDto, NhaCungCapGetAllInputDto, NhaCungCapServiceProxy } from '@shared/service-proxies/service-proxies';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { CreateOrEditNhaCungCapComponent } from './create-or-edit-nha-cung-cap/create-or-edit-nha-cung-cap.component';

const URL = AppConsts.remoteServiceBaseUrl + '/api/Upload/NhaCungCapUpload';
@Component({
  selector: 'app-nha-cung-cap',
  templateUrl: './nha-cung-cap.component.html',
  animations: [appModuleAnimation()],
})
export class NhaCungCapComponent extends AppComponentBase implements OnInit {

  @ViewChild('dt') table: Table;
  loading = true;
  exporting = false;
  keyword = '';
  totalCount = 0;
  records: NhaCungCapForViewDto[] = [];
  input: NhaCungCapGetAllInputDto;
  listLinhVuc: LookupTableDto[] = [];
  form: FormGroup;
  constructor(
    injector: Injector,
    private _http: HttpClient,
    private _modalService: BsModalService,
    private _fileDownloadService: FileDownloadService,
    private _nhaCungCapServiceProxy: NhaCungCapServiceProxy,
    private _lookupTableServiceProxy: LookupTableServiceProxy,
    private _fb: FormBuilder,
  ) { super(injector); }


  ngOnInit(): void {
    forkJoin(
      this._lookupTableServiceProxy.getAllLinhVucKinhDoanh(),
    ).subscribe(([linhVuc]) => {
      this.listLinhVuc = linhVuc;
    });
    this.khoiTaoForm();
  }

  khoiTaoForm() {
    this.form = this._fb.group({
      keyword: [''],
      LinhVuc: [''],
    });
  }

  getDataPage(isSearch: boolean, lazyLoad?: LazyLoadEvent) {
    this.loading = true;
    this._nhaCungCapServiceProxy.getAll(
      this.form.controls.keyword.value || undefined,
      this.form.controls.LinhVuc.value?.id || undefined,
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

  delete(record: NhaCungCapForViewDto) {
    const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
    '<p class="text-popup-xoa m-t-8">'
     + 'Nhà cung cấp ' + record.nhaCungCap.tenNhaCungCap + ' sẽ bị xóa!' + '</p>';
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
        this._nhaCungCapServiceProxy.delete(record.nhaCungCap.id).subscribe(() => {
          this.showDeleteMessage();
          this.getDataPage(false);
        });
      }
    });
  }

  importExcel() {
    this._showImportDemoDialog();
  }

  exportToExcel() {
    this.exporting = true;
    this.input = new NhaCungCapGetAllInputDto();
    this.input.keyword = this.form.controls.keyword.value || undefined;
    this.input.linhVuc = this.form.controls.LinhVuc.value?.id || undefined;
    this.input.sorting = this.getSortField(this.table);
    this.input.skipCount = 0;

    this.input.maxResultCount = 10000000;
    this._nhaCungCapServiceProxy.exportToExcel(this.input).subscribe((result) => {
      this._fileDownloadService.downloadTempFile(result);
      this.exporting = false;
    }, () => {
      this.exporting = false;
    });
  }

  private _showCreateOrEditDemoDialog(id?: number, isView = false): void {
    // copy
    let createOrEditUserDialog: BsModalRef;
    createOrEditUserDialog = this._modalService.show(
      CreateOrEditNhaCungCapComponent,
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
      this._nhaCungCapServiceProxy.downloadFileMau().subscribe(result => {
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
        this._nhaCungCapServiceProxy.importFileExcel(res['result'][0]).subscribe((message) => {
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

}
