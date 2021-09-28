import { Component, Injector, Input, OnChanges, OnInit, SimpleChanges, ViewChild } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import {
  GetAllInputDto,
  GetAllOutputDto,
  LookupTableDto,
  LookupTableServiceProxy,
  ToanBoTaiSanServiceProxy,
} from '@shared/service-proxies/service-proxies';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { Table } from 'primeng/table';
import { LazyLoadEvent } from 'primeng/api';
import { CreateOrEditTaiSanComponent } from './create-or-edit-tai-san/create-or-edit-tai-san.component';
import { finalize } from 'rxjs/operators';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { FormBuilder, FormGroup } from '@angular/forms';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { FileDownloadService } from '@shared/file-download.service';
import { ImportExcelDialogComponent } from '@shared/components/import-excel/import-excel-dialog.component';
import { HttpClient } from '@angular/common/http';
import { AppConsts } from '@shared/AppConsts';
import { PermissionTreeEditModel } from '@shared/dft/components/permission-tree-edit.model';

const URL = AppConsts.remoteServiceBaseUrl + '/api/Upload/ToanBoTSUpload';

@Component({
  selector: 'app-toan-bo-tai-san',
  templateUrl: './toan-bo-tai-san.component.html',
  styleUrls: ['./toan-bo-tai-san.component.scss'],
  animations: [appModuleAnimation()],

})
export class ToanBoTaiSanComponent extends AppComponentBase implements OnInit, OnChanges {
  @Input() isActive = 0;
  @ViewChild('dt') table: Table;
  loading = true;
  exporting = false;
  totalCount = 0;
  first = 0;
  records: GetAllOutputDto[] = [];
  nhaCC: LookupTableDto[];
  maSD: LookupTableDto[];
  ttSD: LookupTableDto[];
  form: FormGroup;
  keyword: '';
  selectedList: GetAllOutputDto[] = [];
  loaiTaiSanItems: TreeviewItem[];
  loaiTaiSanValue: number;
  input: GetAllInputDto;

  toChucItems: PermissionTreeEditModel;
  toChucValue: number[];

  titleNotice = 'Bạn chắc chắn không?';

  listIdXoa: number[] = [];

  constructor(injector: Injector,
    private _modalService: BsModalService,
    private _toanBoTaiSanServiceProxy: ToanBoTaiSanServiceProxy,
    private _fb: FormBuilder,
    private _lookupTableService: LookupTableServiceProxy,
    private _fileDownloadService: FileDownloadService,
    private _http: HttpClient,
  ) { super(injector); }

  ngOnInit(): void {
    this.khoiTaoForm();
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
    this._toanBoTaiSanServiceProxy.getAllNhaCC().subscribe(result => {
      this.nhaCC = result;
    });
    this._lookupTableService.getAllTinhTrangMaSuDungTaiSan().subscribe(result => {
      this.maSD = result;
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.isActive?.currentValue === 0 && this.form && this.table) {
      this.getDataPage(false);
      this.selectedList = [];
    }
  }

  khoiTaoForm() {
    this.form = this._fb.group({
      PBQLFilter: [],
      LoaiTSFilter: [],
      NhaCCFilter: [],
      TinhTrangFilter: [],
      MaSDFilter: [],
    });
  }

  getDataPage(isSearch: boolean, lazyLoad?: LazyLoadEvent) {
    this.first = 0;
    this.loading = true;
    this._toanBoTaiSanServiceProxy.getAll(
      this.keyword || undefined,
      this.toChucValue,
      this.loaiTaiSanValue,
      this.form?.controls.NhaCCFilter.value?.id,
      this.form?.controls.TinhTrangFilter.value?.id,
      this.form?.controls.MaSDFilter.value?.id,
      isSearch,
      this.getSortField(this.table),
      lazyLoad ? lazyLoad.first : this.first,
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

  searchTTSD(event) {

    const query = event.query;
    this._lookupTableService.getAllTrangThaiTaiSan().subscribe(result => {
      this.ttSD = this.filterTrangThaiDuyet(query, result);
    });
  }

  filterTrangThaiDuyet(query, trangThai: LookupTableDto[]): any[] {
    const filtered: any[] = [];
    for (const iterator of trangThai) {
      if (iterator.displayName.toLowerCase().indexOf(query.toLowerCase()) === 0) {
        filtered.push(iterator);
      }
    }
    return filtered;
  }
  delete(record: GetAllOutputDto) {
    if (record.trangThaiId === 0) {
      const html1 = '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
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
          this._toanBoTaiSanServiceProxy.deleted(record.id).subscribe(() => {
            this.showDeleteMessage();
            this.getDataPage(false);
          });
        }
      });
    } else {
      this._thongBao('Tài sản đang được sử dụng, không được xóa!');
    }
  }

  xoaList(res: GetAllOutputDto[]) {
    const listResult = res.filter(f => f.trangThaiId === 0);

    listResult.forEach(item => {
      this.listIdXoa.push(item.id);
    });
    if (this.listIdXoa.length > 0) {
      const html1 = '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
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
          this._toanBoTaiSanServiceProxy.xoaList(this.listIdXoa).subscribe(() => {
            this.showDeleteMessage();
            this.getDataPage(false);
            this.selectedList = [];
          });
        }
      });
    } else {
      this._thongBao('Tài sản đang được sử dụng, không được xóa!');
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
      this._toanBoTaiSanServiceProxy.downloadFileMau().subscribe(result => {
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
        this._toanBoTaiSanServiceProxy.importFileExcel(res['result'][0]).subscribe((message) => {
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
    this.input = new GetAllInputDto();

    this.input.fillter = this.keyword || undefined;
    this.input.phongBanqQL = this.toChucValue;
    this.input.loaiTS = this.loaiTaiSanValue;
    this.input.nhaCungCap = this.form?.controls.NhaCCFilter.value?.id;
    this.input.tinhTrangSD = this.form?.controls.TinhTrangFilter.value?.id;
    this.input.maSD = this.form?.controls.MaSDFilter.value?.id;
    this.input.sorting = this.getSortField(this.table);

    this.input.skipCount = 0;
    this.input.maxResultCount = 10000000;
    this._toanBoTaiSanServiceProxy.exportToExcel(this.input).subscribe((result) => {
      this._fileDownloadService.downloadTempFile(result);
      this.exporting = false;
    }, () => {
      this.exporting = false;
    });
  }

  private _thongBao(textExit: string) {
    this.showExistMessage(textExit);
  }
}
