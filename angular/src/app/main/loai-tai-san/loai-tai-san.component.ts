import { HttpClient } from '@angular/common/http';
import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { AppConsts } from '@shared/AppConsts';
import { ImportExcelDialogComponent } from '@shared/components/import-excel/import-excel-dialog.component';
import { FileDownloadService } from '@shared/file-download.service';
import { LoaiTaiSanForViewDto, LoaiTaiSanGetAllInputDto, LoaiTaiSanServiceProxy, LoaiTaiSanTreeTableForViewDto } from '@shared/service-proxies/service-proxies';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';
import { finalize } from 'rxjs/operators';
import { CreateOrEditLoaiTaiSanComponent } from './create-or-edit-loai-tai-san/create-or-edit-loai-tai-san.component';

const URL = AppConsts.remoteServiceBaseUrl + '/api/Upload/LoaiTaiSanUpload';
@Component({
  selector: 'app-loai-tai-san',
  templateUrl: './loai-tai-san.component.html',
  styleUrls: ['./loai-tai-san.component.scss'],
  animations: [appModuleAnimation()],
})
export class LoaiTaiSanComponent extends AppComponentBase implements OnInit {

  @ViewChild('dt') table: Table;
  loading = true;
  exporting = false;
  keyword = '';
  totalCount = 0;
  records: LoaiTaiSanTreeTableForViewDto[] = [];
  input: LoaiTaiSanGetAllInputDto;
  files1 = [];

  constructor(
    injector: Injector,
    private _http: HttpClient,
    private _modalService: BsModalService,
    private _fileDownloadService: FileDownloadService,
    private _loaiTaiSanServiceProxy: LoaiTaiSanServiceProxy,
  ) { super(injector); }

  ngOnInit(): void {
    this.getDataPage(false);
  }

  getDataPage(isSearch: boolean) {
    this.loading = true;
    this._loaiTaiSanServiceProxy.getAll(
      this.keyword || undefined,
      isSearch,
    ).pipe(finalize(() => { this.loading = false; }))
      .subscribe(result => {
        this.files1 = result;
        this.records = result;
        this.totalCount = result.length;
      });
  }

  create(id?: number) {
    this._showCreateOrEditLoaiTaiSanDialog(id);
  }

  view(id?: number) {
    this._showCreateOrEditLoaiTaiSanDialog(id, true);
  }

  delete(record: LoaiTaiSanForViewDto, row: LoaiTaiSanTreeTableForViewDto) {
    if (row.children?.length > 0) {
      this.showExistMessage('Loại tài sản cha không thể xóa!');
    } else {
      const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
      '<p class="text-popup-xoa m-t-8">'
       + 'Loại tài sản ' + record.loaiTaiSan.ten + ' sẽ bị xóa!' + '</p>';
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
          this._loaiTaiSanServiceProxy.delete(record.loaiTaiSan.id).subscribe((res) => {
            if (res === 1) {
              this.showExistMessage('Loại tài sản đã có tài sản, không được xóa!');
            } else {
              this.showDeleteMessage();
              this.getDataPage(false);
            }
          });
        }
      });
    }
  }

  importExcel() {
    this._showImportLoaiTaiSanDialog();
  }

  exportToExcel() {
    this.exporting = true;
    this.input = new LoaiTaiSanGetAllInputDto();
    this.input.keyword = this.keyword || undefined;
    this._loaiTaiSanServiceProxy.exportToExcel(this.input).subscribe((result) => {
      this._fileDownloadService.downloadTempFile(result);
      this.exporting = false;
    }, () => {
      this.exporting = false;
    });
  }

  private _showCreateOrEditLoaiTaiSanDialog(id?: number, isView = false): void {
    // copy
    let createOrEditDialog: BsModalRef;
    createOrEditDialog = this._modalService.show(
      CreateOrEditLoaiTaiSanComponent,
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
    createOrEditDialog.content.onSave.subscribe(() => {
      this.getDataPage(false);
    });
  }

  private _showImportLoaiTaiSanDialog(): void {
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
      this._loaiTaiSanServiceProxy.downloadFileMau().subscribe(result => {
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
        this._loaiTaiSanServiceProxy.importFileExcel(res['result'][0]).subscribe((message) => {
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
