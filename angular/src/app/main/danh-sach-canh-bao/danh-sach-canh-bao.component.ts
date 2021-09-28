import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { HttpClient } from '@aspnet/signalr/dist/esm/HttpClient';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { PermissionTreeEditModel } from '@shared/dft/components/permission-tree-edit.model';
import { FileDownloadService } from '@shared/file-download.service';
import { CanhBaoForViewDto, CanhBaoInputDto, CanhBaoServiceProxy, LookupTableDto, LookupTableServiceProxy, TreeviewItemDto } from '@shared/service-proxies/service-proxies';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-danh-sach-canh-bao',
  templateUrl: './danh-sach-canh-bao.component.html',
  styleUrls: ['./danh-sach-canh-bao.component.scss'],
  animations: [appModuleAnimation()],
})
export class DanhSachCanhBaoComponent extends AppComponentBase implements OnInit {

  @ViewChild('dt') table: Table;

  readStateFilter = 'ALL';
  loading = true;
  records: CanhBaoForViewDto[] = [];
  totalCount = 0;
  toChuc: string;
  exporting = false;

  form: FormGroup;
  loaiTaiSanValue: number;

  toChucValue: number[];
  toChucItems: PermissionTreeEditModel;
  listHoatDong: LookupTableDto[];
  listNguoiDung: LookupTableDto[];

  input: CanhBaoInputDto;

  constructor(
    injector: Injector,
    private _notificationService: CanhBaoServiceProxy,
    private _fb: FormBuilder,
    private _lookupTableService: LookupTableServiceProxy,
    private _fileDownloadService: FileDownloadService,
  ) {
    super(injector);
  }
  ngOnInit(): void {
    this.khoiTaoForm();
    forkJoin(
      this._lookupTableService.getAllHoatDong(),
      this._notificationService.getAllNguoiDung(),
      this._lookupTableService.getAllToChucTheoNguoiDung(),
    ).subscribe(([hoatDong, nguoiDung, toChuc]) => {
      this.listHoatDong = hoatDong;
      this.listNguoiDung = nguoiDung;
      this.toChucItems = {
        data: toChuc,
        selectedData: toChuc.map(e => e.id),
      };
      this.toChucValue = toChuc.map(e => e.id);
    });
  }

  khoiTaoForm() {
    this.form = this._fb.group({
      keyword: [''],
      PhongBan: [''],
      HoatDong: [''],
      ThoiGian: [''],
      NguoiDung: [''],
    });
  }

  getDataPage(isSearch: boolean, lazyLoad?: LazyLoadEvent): void {
    this.loading = true;
    this._notificationService.getAll(
      this.form.controls.NguoiDung.value?.id || undefined,
      this.toChucValue,
      this.form.controls.keyword.value || undefined,
      this.form.controls.HoatDong.value?.id || undefined,
      this.form.controls.ThoiGian.value ? this.form.controls.ThoiGian.value[0] : undefined,
      this.form.controls.ThoiGian.value ? this.form.controls.ThoiGian.value[1] : undefined,
      isSearch,
      this.getSortField(this.table),
      lazyLoad ? lazyLoad.first : 0,
      lazyLoad ? lazyLoad.rows : this.table.rows,
    ).subscribe((result) => {
      this.totalCount = result.totalCount;
      this.records = result.items;
      this.loading = false;
    });
  }

  setControlValue(value) {
    this.form.get('PhongBan').setValue(value);
  }

  exportToExcel() {
    this.exporting = true;
    this.input = new CanhBaoInputDto();
    this.input.taiKhoanId = this.form.controls.NguoiDung.value?.id || undefined;
    this.input.toChucId = this.toChucValue;
    this.input.noiDung = this.form.controls.keyword.value || undefined;
    this.input.hoatDong = this.form.controls.HoatDong.value?.id || undefined;
    this.input.thoiGianFrom = this.form.controls.ThoiGian.value ? this.form.controls.ThoiGian.value[0] : undefined;
    this.input.thoiGianTo = this.form.controls.ThoiGian.value ? this.form.controls.ThoiGian.value[1] : undefined;
    this.input.sorting = this.getSortField(this.table);
    this.input.skipCount = 0;
    this.input.maxResultCount = 10000000;
    this._notificationService.exportToExcel(this.input).subscribe((result) => {
      this._fileDownloadService.downloadTempFile(result);
      this.exporting = false;
    }, () => {
      this.exporting = false;
    });
  }
}
