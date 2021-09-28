import { HttpClient } from '@angular/common/http';
import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { AppConsts } from '@shared/AppConsts';
import { FileDownloadService } from '@shared/file-download.service';
import { DatLichXuatBaoCaoServiceProxy, GetAllDatLichBCDtos, LookupTableDto, LookupTableServiceProxy, } from '@shared/service-proxies/service-proxies';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { CreateOrEditDatLichComponent } from './create-or-edit-dat-lich/create-or-edit-dat-lich.component';
const URL = AppConsts.remoteServiceBaseUrl + '/api/Upload/NhaCungCapUpload';

@Component({
  selector: 'app-dat-lich-xuat-bao-cao',
  templateUrl: './dat-lich-xuat-bao-cao.component.html',
  styleUrls: ['./dat-lich-xuat-bao-cao.component.scss'],
  animations: [appModuleAnimation()],
})
export class DatLichXuatBaoCaoComponent extends AppComponentBase implements OnInit {
  @ViewChild('dt') table: Table;
  loading = true;
  exporting = false;
  keyword = '';
  totalCount = 0;
  records: GetAllDatLichBCDtos[] = [];
  input: GetAllDatLichBCDtos;
  listLinhVuc: LookupTableDto[] = [];
  form: FormGroup;
  constructor(
    injector: Injector,
    private _http: HttpClient,
    private _modalService: BsModalService,
    private _datLichServiceProxy: DatLichXuatBaoCaoServiceProxy,
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
    this._datLichServiceProxy.getAllDatLich(
      undefined,
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

  delete(record: GetAllDatLichBCDtos) {
    const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
    '<p class="text-popup-xoa m-t-8">'
     + 'Lịch xuất ' + record.tenBaoCao + ' sẽ bị xóa!' + '</p>';
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
        this._datLichServiceProxy.delete(record.id).subscribe(() => {
          this.showDeleteMessage();
          this.getDataPage(true);
        });
      }
    });
  }

  private _showCreateOrEditDemoDialog(id?: number, isView = false): void {
    // copy
    let createOrEditUserDialog: BsModalRef;
    createOrEditUserDialog = this._modalService.show(
      CreateOrEditDatLichComponent,
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
}
