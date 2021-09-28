import { ViewChild, Injector } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AppComponentBase } from '@shared/app-component-base';
import { PermissionTreeEditModel } from '@shared/dft/components/permission-tree-edit.model';
import { FileDownloadService } from '@shared/file-download.service';
import { LichSuRaVaoForViewDto, LichSuRaVaoAngtenServiceProxy, ToanBoTaiSanServiceProxy, InputLichSuRaVaoDto, LookupTableServiceProxy } from '@shared/service-proxies/service-proxies';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';
import { finalize } from 'rxjs/operators';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { CreateOrEditTaiSanComponent } from '@app/main/tai-san/toan-bo-tai-san/create-or-edit-tai-san/create-or-edit-tai-san.component';

@Component({
  selector: 'app-theo-doi-ket-noi-thiet-bi',
  templateUrl: './theo-doi-ket-noi-thiet-bi.component.html',
  styleUrls: ['./theo-doi-ket-noi-thiet-bi.component.scss']
})
export class TheoDoiKetNoiThietBiComponent extends AppComponentBase implements OnInit {
  @ViewChild('dt') table: Table;
  selectedList: LichSuRaVaoForViewDto[] = [];
  loading = true;
  totalCount = 0;
  form: FormGroup;
  records: LichSuRaVaoForViewDto[] = [];
  input: InputLichSuRaVaoDto;
  createInputDto: any = {
    TaiSanId: 0,
    RFID: ''
  };
  exporting = false;

  toChucItems: PermissionTreeEditModel;
  listChieuDiChuyen: any[] = [];
  listPhanLoaiTaiSan: any[] = [];
  toChucValue: number[];
  constructor(injector: Injector,
    private _lichsuRaVaoAngtenServiceProxy: LichSuRaVaoAngtenServiceProxy,
    private _toanBoTaiSanServiceProxy: ToanBoTaiSanServiceProxy,
    private _fb: FormBuilder,
    private _modalService: BsModalService,
    private _lookupTableService: LookupTableServiceProxy,
    private _fileDownloadService: FileDownloadService,
  ) {
    super(injector);
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



    this._lookupTableService.getAllPhanLoaiTaiSanTrongHeThong().subscribe((loaiTaiSan) => {
      this.listPhanLoaiTaiSan = loaiTaiSan;
    });

    this._lookupTableService.getAllChieuTaiSanDiChuyen().subscribe((chieu) => {
      this.listChieuDiChuyen = chieu;
    });
    this.khoiTaoForm();
  }

  khoiTaoForm() {
    this.form = this._fb.group({
      keyword: [''],
      PhongBan: [''],
      PhanLoaiFilter: [''],
      chieuDiChuyen: [''],
      DateFromTo: [],
    });
  }

  getDataPage(isSearch: boolean, lazyLoad?: LazyLoadEvent) {
    this.selectedList = [];
    this.loading = true;
    this._lichsuRaVaoAngtenServiceProxy.getAllTaiSanLa(
      this.form.controls.keyword.value || undefined,
      this.form.controls.DateFromTo.value ? this.form.controls.DateFromTo.value[0] : null,
      this.form.controls.DateFromTo.value ? this.form.controls.DateFromTo.value[1] : null,
      this.form.controls.chieuDiChuyen.value?.id,
      this.toChucValue,
      this.form.controls.PhanLoaiFilter.value?.id,
      isSearch,
      this.getSortField(this.table),
      lazyLoad ? lazyLoad.first : 0,
      lazyLoad ? lazyLoad.rows : this.table.rows,
    ).pipe(finalize(() => { this.loading = false; }))
      .subscribe(result => {
        this.records = result.items;
        this.totalCount = result.totalCount;
      });
  }

  exportToExcel() {
    this.exporting = true;
    this.input = new InputLichSuRaVaoDto();
    this.input.keyword = this.form.controls.keyword.value || undefined;
    this.input.startDate = this.form.controls.DateFromTo.value ? this.form.controls.DateFromTo.value[0] : null;
    this.input.endDate = this.form.controls.DateFromTo.value ? this.form.controls.DateFromTo.value[1] : null;
    this.input.chieuDiChuyen = this.form.controls.chieuDiChuyen.value?.id;
    this.input.boPhanId = this.toChucValue;
    this.input.phanLoaiId = this.form.controls.PhanLoaiFilter.value?.id;
    this.input.skipCount = 0;
    this.input.maxResultCount = 10000000;
    this._lichsuRaVaoAngtenServiceProxy.exportTaiSanLaToExcel(this.input).subscribe((result) => {
      this._fileDownloadService.downloadTempFile(result);
      this.exporting = false;
    }, () => {
      this.exporting = false;
    });
  }


  create(rfid?: string) {
    this._showCreateOrEditDemoDialog(rfid);
  }

  private _showCreateOrEditDemoDialog(rfid?: string, isView = false): void {
    // copy
    let createOrEditUserDialog: BsModalRef;
    createOrEditUserDialog = this._modalService.show(
      CreateOrEditTaiSanComponent,
      {
        class: 'modal-xl',
        ignoreBackdropClick: true,
        initialState: {
          isView,
        },
      }
    );

    // ouput emit
    createOrEditUserDialog.content.onSave.subscribe((result) => {
      this.createInputDto.TaiSanId = result;
      this.createInputDto.RFID = rfid;
      this._lichsuRaVaoAngtenServiceProxy.update(this.createInputDto).subscribe(() => {
        this.getDataPage(false);
      });
    });
  }

}
