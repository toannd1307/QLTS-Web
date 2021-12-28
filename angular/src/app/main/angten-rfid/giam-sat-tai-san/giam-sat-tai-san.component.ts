import { ViewChild, Injector } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { PermissionTreeEditModel } from '@shared/dft/components/permission-tree-edit.model';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { FileDownloadService } from '@shared/file-download.service';
import { LichSuRaVaoForViewDto, LichSuRaVaoAngtenServiceProxy, LookupTableServiceProxy, InputLichSuRaVaoDto } from '@shared/service-proxies/service-proxies';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-giam-sat-tai-san',
  templateUrl: './giam-sat-tai-san.component.html',
  styleUrls: ['./giam-sat-tai-san.component.scss'],
  animations: [appModuleAnimation()],
})
export class GiamSatTaiSanComponent extends AppComponentBase implements OnInit {

  @ViewChild('dt') table: Table;
  loading = true;
  totalCount = 0;
  form: FormGroup;
  records: LichSuRaVaoForViewDto[] = [];
  input: InputLichSuRaVaoDto;
  exporting = false;

  toChucItems: PermissionTreeEditModel;
  listChieuDiChuyen: any[] = [];
  listPhanLoaiTaiSan: any[] = [];
  toChucValue: number[] = [];
  constructor(injector: Injector,
    private _lichsuRaVaoAngtenServiceProxy: LichSuRaVaoAngtenServiceProxy,
    private _fb: FormBuilder,
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
    if (isSearch) {
      this.loading = true;
      this._lichsuRaVaoAngtenServiceProxy.getAll(
        this.form.controls.keyword.value || undefined,
        this.form.controls.DateFromTo.value ? this.form.controls.DateFromTo.value[0] : null,
        this.form.controls.DateFromTo.value ? this.form.controls.DateFromTo.value[1] : null,
        this.form.controls.chieuDiChuyen.value?.displayName,
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
  }

  exportToExcel() {
    this.exporting = true;
    this.input = new InputLichSuRaVaoDto();
    this.input.keyword = this.form.controls.keyword.value || undefined;
    this.input.startDate = this.form.controls.DateFromTo.value ? this.form.controls.DateFromTo.value[0] : null;
    this.input.endDate = this.form.controls.DateFromTo.value ? this.form.controls.DateFromTo.value[1] : null;
    this.input.chieuDiChuyen = this.form.controls.chieuDiChuyen.value?.displayName;
    this.input.boPhanId = this.toChucValue;
    this.input.phanLoaiId = this.form.controls.PhanLoaiFilter.value?.id;
    this.input.skipCount = 0;
    this.input.maxResultCount = 10000000;
    this._lichsuRaVaoAngtenServiceProxy.exportLichSuRaVaoToExcel(this.input).subscribe((result) => {
      this._fileDownloadService.downloadTempFile(result);
      this.exporting = false;
    }, () => {
      this.exporting = false;
    });
  }

}
