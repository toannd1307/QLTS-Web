import { Component, EventEmitter, Injector, OnInit, Output, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { CommonComponent } from '@shared/dft/components/common.component';
import {
  LookupTableDto,
  LookupTableServiceProxy,
  PhieuTaiSanCreateInputDto,
  TaiSanSuaChuaBaoDuongServiceProxy,
  ViewTaiSanSuaChuaBaoDuong,
} from '@shared/service-proxies/service-proxies';
import { finalize, map } from 'rxjs/operators';
import { Table } from 'primeng/table';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { ThemTaiSanSuaChuaBaoDuongComponent } from '../them-tai-san-sua-chua-bao-duong/them-tai-san-sua-chua-bao-duong.component';
import { PhieuTaiSanChiTietDto } from './../../../../../shared/service-proxies/service-proxies';
import { spaceValidator } from '@shared/dft/components/validation-messages.component';
import { forkJoin } from 'rxjs';


@Component({
  selector: 'app-create-or-edit-tai-san-sua-chua-bao-duong',
  templateUrl: './create-or-edit-tai-san-sua-chua-bao-duong.component.html',
  styleUrls: ['./create-or-edit-tai-san-sua-chua-bao-duong.component.scss'],
  animations: [appModuleAnimation()],
})
export class CreateOrEditTaiSanSuaChuaBaoDuongComponent extends AppComponentBase implements OnInit {

  @Output() onSave = new EventEmitter<any>();
  form: FormGroup;
  saving = false;
  isEdit = false;
  id: number;
  isView = false;
  createInputDto: PhieuTaiSanCreateInputDto = new PhieuTaiSanCreateInputDto();
  @ViewChild('dt') table: Table;
  loading = true;
  exporting = false;
  keyword = '';
  totalCount = 0;
  records: any[] = [];
  suggestionsSingle: LookupTableDto[];
  formSearch: FormGroup;
  arrHinhThuc: any[] = [];
  arrTaiSanChecked: ViewTaiSanSuaChuaBaoDuong[] = [];


  constructor(
    injector: Injector,
    private _modalService: BsModalService,
    private _taiSanServiceProxy: TaiSanSuaChuaBaoDuongServiceProxy,
    private _lookupTableService: LookupTableServiceProxy,
    private _fb: FormBuilder,
    public bsModalRef: BsModalRef,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.khoiTaoForm();
    forkJoin(
      this._lookupTableService.getAllTrangThaiTaiSan(),
    ).subscribe(([trangThaiTaiSan]) => {
      this.arrHinhThuc = trangThaiTaiSan.filter(ff =>
        ff.id === this.enums.TrangThaiTaiSanConst.TaiSanSuaChua ||
        ff.id === this.enums.TrangThaiTaiSanConst.TaiSanBaoDuong);
    });
  }

  create(id?: number) {
    this._showCreateOrEditDemoDialog(id);
  }

  khoiTaoForm() {
    this.form = this._fb.group({
      hinhThuc: ['', Validators.required],
      thoiGianBatDau: [new Date(), Validators.required],
      noiDungKhaiBaoSuaChuaBaoDuong: ['', [Validators.required, Validators.maxLength(4000), spaceValidator()]],
      diaChiSuaChuaBaoDuong: ['', [Validators.maxLength(4000), spaceValidator()]],
    });
  }

  search(event, item) {
    const query = event.query;
    if (item === 'hinhThuc') {
      this._lookupTableService.getAllTrangThaiTaiSan().pipe(
        map(f => f.filter(ff =>
          ff.id === this.enums.TrangThaiTaiSanConst.TaiSanSuaChua ||
          ff.id === this.enums.TrangThaiTaiSanConst.TaiSanBaoDuong)),
      ).subscribe(result => {
        this.suggestionsSingle = this.filter(query, result);
      });
    }
  }

  filter(query, items: LookupTableDto[]): any[] {
    const filtered: any[] = [];
    for (const iterator of items) {
      if (iterator.displayName.toLowerCase().indexOf(query.toLowerCase()) === 0) {
        filtered.push(iterator);
      }
    }
    return filtered;
  }

  deleteArrTaiSanChecked() {
    // tslint:disable-next-line: no-commented-code
    // const tenTaiSan = this.arrTaiSanChecked.map(m => { return m.tenTaiSan; }).join('<br/>');
    // this.swal.fire({
    //   title: 'Bạn có chắc chắn không?',
    //   html: 'Các tài sản: ' + '<br/>' + tenTaiSan + '<br/>' + ' sẽ bị xóa!',
    //   icon: 'warning',
    //   showCancelButton: true,
    //   confirmButtonColor: this.confirmButtonColor,
    //   cancelButtonColor: this.cancelButtonColor,
    //   cancelButtonText: this.cancelButtonText,
    //   confirmButtonText: this.confirmButtonText
    // }).then((result) => {
    //   if (result.value) {
    //     this.deleteCheckedItems();
    //     this.showDeleteMessage();
    //   }
    // });
    this.deleteCheckedItems();
    this.showDeleteMessage();
  }

  deleteCheckedItems() {
    this.records = this.records.filter(f =>
      !this.arrTaiSanChecked.some(s => s.id === f.id)
    );
    this.arrTaiSanChecked = [];
  }

  private _showCreateOrEditDemoDialog(id?: number, isView = false): void {
    // copy
    let createOrEditUserDialog: BsModalRef;
    createOrEditUserDialog = this._modalService.show(
      ThemTaiSanSuaChuaBaoDuongComponent,
      {
        class: 'modal-xl',
        ignoreBackdropClick: true,
        initialState: {
          id,
          isView,
          arrTaiSanCheckedFromParent: this.records
        },
      }
    );

    // ouput emit
    createOrEditUserDialog.content.onSave.subscribe(() => {
      this.records = [...this.records, ...createOrEditUserDialog.content.arrTaiSanChecked];
      this.totalCount = this.records.length;
    });
  }

  save(): void {
    if (CommonComponent.getControlErr(this.form) === '') {
      this.saving = true;
      this._getValueForSave();
      this._taiSanServiceProxy.createTaiSanSuaChuaBaoDuong(this.createInputDto).pipe(
        finalize(() => {
          this.saving = false;
        })
      ).subscribe((result) => {
        if (result === 1) {
          this.showExistMessage('Mã nhà cung cấp đã bị trùng!');
        } else if (!this.id) {
          this.showCreateMessage();
          this.bsModalRef.hide();
          this.onSave.emit();
        } else {
          this.showUpdateMessage();
          this.bsModalRef.hide();
          this.onSave.emit();
        }
      });
    }
  }

  close() {
    this.bsModalRef.hide();
  }

  private _getValueForSave() {
    const items: PhieuTaiSanChiTietDto[] = [];
    this.records.forEach(e => {
      const item = new PhieuTaiSanChiTietDto();
      item.taiSanId = e.id;
      item.trangThaiId = this.enums.TrangThaiSuaChuaBaoDuongConst.DangThucHien;
      items.push(item);
    });
    this.createInputDto = {
      ...this.form.value,
      phanLoaiId: this.form.value.hinhThuc.id,
      ngayKhaiBao: this.form.value.thoiGianBatDau,
      noiDung: this.form.value.noiDungKhaiBaoSuaChuaBaoDuong,
      diaChi: this.form.value.diaChiSuaChuaBaoDuong,
      phieuTaiSanChiTietList: items
    };
  }
}
