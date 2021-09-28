import { Component, Injector, Input, OnInit, ViewChild } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { KetQuaKiemKeForUpdateDto, KetQuaKiemKeForViewDto, KiemKeTaiSanServiceProxy } from '@shared/service-proxies/service-proxies';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';
import { finalize } from 'rxjs/operators';
import { CreateOrEditTaiSanComponent } from '../../../tai-san/toan-bo-tai-san/create-or-edit-tai-san/create-or-edit-tai-san.component';

@Component({
  selector: 'app-danh-sach-kiem-ke-tai-san',
  templateUrl: './danh-sach-kiem-ke-tai-san.component.html',
  styleUrls: ['./danh-sach-kiem-ke-tai-san.component.scss']
})

export class DanhSachKiemKeTaiSanComponent extends AppComponentBase implements OnInit {
  @Input() status = '';
  @Input() id: number;
  @Input() isDisable: boolean;
  @ViewChild('dt') table: Table;
  isFound = false;
  isNotFound = false;
  isNotInList = false;
  loading = true;
  keyword = '';
  updateInput: KetQuaKiemKeForUpdateDto;
  updateInputDto: KetQuaKiemKeForUpdateDto[] = [];
  foundRecords: KetQuaKiemKeForViewDto[] = [];
  notFoundRecords: KetQuaKiemKeForViewDto[] = [];
  unknownRecords: KetQuaKiemKeForViewDto[] = [];
  selectedRows: any[];


  constructor(
    injector: Injector,
    private _kiemKeTaiSanServiceProxy: KiemKeTaiSanServiceProxy,
    private _modalService: BsModalService,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    console.log(this.status);
    switch (this.status) {
      case '0':
        this.isFound = true;
        this._kiemKeTaiSanServiceProxy.getAllTaiSan(this.id, 0, this.keyword || undefined, this.getSortField(this.table), false)
          .pipe(finalize(() => { this.loading = false; })).subscribe(item => {
            this.foundRecords = item;
            console.log(this.status);
          });
        break;
      case '1':
        this.isNotFound = true;
        this._kiemKeTaiSanServiceProxy.getAllTaiSan(this.id, 1, this.keyword || undefined, this.getSortField(this.table), false)
          .pipe(finalize(() => { this.loading = false; })).subscribe(item => {
            this.notFoundRecords = item;
            console.log(this.status);
          });
        break;
      case '2':
        this.isNotInList = true;
        this._kiemKeTaiSanServiceProxy.getAllTaiSan(this.id, 2, this.keyword || undefined, this.getSortField(this.table), false)
          .pipe(finalize(() => { this.loading = false; })).subscribe(item => {
            this.unknownRecords = item;
            console.log(this.status);
          });
        break;
      default:
        break;
    }
  }
  getDataPage(isSearch: boolean, lazyLoad?: LazyLoadEvent) {
    this.loading = true;
    switch (this.status) {
      case '0':
        this._kiemKeTaiSanServiceProxy.getAllTaiSan(this.id, 0, this.keyword || undefined, this.getSortField(this.table), isSearch)
          .pipe(finalize(() => { this.loading = false; })).subscribe(item => {
            this.foundRecords = item;
          });
        break;
      case '1':
        this._kiemKeTaiSanServiceProxy.getAllTaiSan(this.id, 1, this.keyword || undefined, this.getSortField(this.table), isSearch)
          .pipe(finalize(() => { this.loading = false; })).subscribe(item => {
            this.notFoundRecords = item;
          });
        break;
      case '2':
        this._kiemKeTaiSanServiceProxy.getAllTaiSan(this.id, 2, this.keyword || undefined, this.getSortField(this.table), isSearch)
          .pipe(finalize(() => { this.loading = false; })).subscribe(item => {
            this.unknownRecords = item;
          });
        break;
      default:
        break;
    }
  }
  isFounded() {
    if (!this.selectedRows || this.selectedRows.length === 0) {
      return true;
    }
    return false;
  }
  founded() {
    this.selectedRows.forEach(e => {
      const update = new KetQuaKiemKeForUpdateDto();
      update.id = e.id;
      update.tinhTrang = 1;
      this.updateInputDto.push(update);
    });
    const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
    '<p class="text-popup-xoa m-t-8">'
     + 'Tài sản được chọn sẽ chuyển sang tìm thấy' + '</p>';
    this.swal.fire({
      html: html1,
      icon: 'warning',
      iconHtml: '<span class="icon1">&#9888</span>',
      showCancelButton: true,
      confirmButtonColor: this.confirmButtonColor,
      cancelButtonColor: this.cancelButtonColor,
      cancelButtonText: this.cancelButtonText,
      confirmButtonText: 'Đồng ý'
    }).then((result) => {
      if (result.value) {
        this._kiemKeTaiSanServiceProxy.updateTinhTrangTaiSan(this.updateInputDto).subscribe(() => {
          this.showUpdateMessage();
          this.getDataPage(false);
          this.foundRecords = [];
          this.updateInputDto = [];
        });
      }
    });
  }
  addTaiSanLa(id) {
    this._showAddNguoiKiemKe(id, false);
  }

  private _showAddNguoiKiemKe(id, isView): void {
    // copy
    let addTaiSan: BsModalRef;
    addTaiSan = this._modalService.show(
      CreateOrEditTaiSanComponent,
      {
        class: 'modal-xl',
        ignoreBackdropClick: true,
        initialState: isView ? {
          isView,
          id
        } : {},
      }
    );
    if (!isView) {
      addTaiSan.content.onSave.subscribe((data) => {
        const update = new KetQuaKiemKeForUpdateDto();
        update.id = id;
        update.tinhTrang = 3;
        this.updateInputDto.push(update);
        this._kiemKeTaiSanServiceProxy.updateTinhTrangTaiSan(this.updateInputDto).subscribe((result) => {
          this.getDataPage(false);
          this.updateInputDto = [];
        });
      });
    }
  }
}
