import { AppComponentBase } from 'shared/app-component-base';
import { Component, EventEmitter, Injector, OnInit, Output, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { ThemDieuChuyenTaiSanComponent } from './them-dieu-chuyen-tai-san/them-dieu-chuyen-tai-san.component';
import { LookupTableDto, LookupTableServiceProxy, MailServerServiceProxy, PhieuTaiSanChiTietDto, PhieuTaiSanCreateInputDto,
TaiSanChuaSuDungForViewDto, TaiSanChuaSuDungServiceProxy } from '@shared/service-proxies/service-proxies';
import { Table } from 'primeng/table';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { forkJoin } from 'rxjs';
import { spaceValidator } from '@shared/dft/components/validation-messages.component';
import { CommonComponent } from '@shared/dft/components/common.component';
import { finalize } from 'rxjs/operators';
import { LazyLoadEvent } from 'primeng/api';

@Component({
  selector: 'app-dieu-chuyen-tai-san',
  templateUrl: './dieu-chuyen-tai-san.component.html',
  styleUrls: ['./dieu-chuyen-tai-san.component.scss']
})
export class DieuChuyenTaiSanComponent extends AppComponentBase implements OnInit {
  @ViewChild('dt') table: Table;
  loading = true;
  @Output() onSave = new EventEmitter<any>();
  form: FormGroup;
  records: TaiSanChuaSuDungForViewDto[] = [];
  taiSans: any[] = [];
  flag: number;
  saving = false;
  toChucItems: TreeviewItem[];
  toChucValue: number;
  totalCount = 0;
  arrTaiSanChecked: TaiSanChuaSuDungForViewDto[] = [];
  createInputDto: PhieuTaiSanCreateInputDto = new PhieuTaiSanCreateInputDto();
  count: number;

  constructor(
    injector: Injector,
    private _fb: FormBuilder,
    public bsModalRef: BsModalRef,
    private _lookupTableService: LookupTableServiceProxy,
    private _modalService: BsModalService,
    private _taiSanChuaSuDungServiceProxy: TaiSanChuaSuDungServiceProxy,
    private _sendMail: MailServerServiceProxy
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.khoiTaoForm();
    if (this.count > 0) {
      this.showExistMessage('Danh sách tài sản đã chọn chứa tài sản không được phép điều chuyển');
    }
    if (this.isGranted('Pages.QuanLyTaiSanTong')) {
      forkJoin(
        this._lookupTableService.getAllToChucTree(),
      ).subscribe(([toChuc]) => {
        this.toChucItems = this.getTreeviewItem(toChuc);
      });
    } else {
      forkJoin(
        this._lookupTableService.getAllToChucTheoNguoiDungTree(false),
      ).subscribe(([toChuc]) => {
        this.toChucItems = this.getTreeviewItem(toChuc);
      });
    }
  }

  khoiTaoForm(): void {
    this.form = this._fb.group({
      thoiGianKhaiBao: [new Date(), Validators.required],
      noiDungKhaiBao: ['', [Validators.maxLength(4000), spaceValidator()]],
      PhongBan: ['', Validators.required],
    });
  }

  save(): void {
    if (CommonComponent.getControlErr(this.form) === '') {
      this.saving = true;
      this._getValueForSave();
      this._taiSanChuaSuDungServiceProxy.capPhatTaiSan(this.createInputDto).pipe(
        finalize(() => {
          this.saving = false;
        })
      ).subscribe((result) => {
        if (result === 0) {
          this.showExistMessage('Lỗi');
        } else {
          this.showSuccessMessage('Điều chuyển thành công');
          this._sendMail.sendMailDieuChuyen(this.form.value.PhongBan, this.records).subscribe(rs => {});
          this.bsModalRef.hide();
          this.onSave.emit();
        }
      });
    }
  }

  private _getValueForSave() {
    const items: PhieuTaiSanChiTietDto[] = [];
    this.records.forEach(e => {
      const item = new PhieuTaiSanChiTietDto();
      item.taiSanId = e.id;
      items.push(item);
    });
    this.createInputDto = {
      ...this.form.value,
      phanLoaiId: this.enums.TrangThaiTaiSanConst.DieuChuyen,
      ngayKhaiBao: this.form.value.thoiGianKhaiBao,
      noiDung: this.form.value.noiDungKhaiBao,
      toChucDuocNhanId: this.form.value.PhongBan,
      phieuTaiSanChiTietList: items
    };
  }

  close(): void {
    this.bsModalRef.hide();
  }

  deleteTaiSan(): void {
  }

  addTaiSan(): void {
    if (this.flag === 1) {
      this._showCreateOrEditDemoDialog(1);
    } else {
      this._showCreateOrEditDemoDialog(2);
    }
  }

  private _showCreateOrEditDemoDialog(flag: number): void {
    let createOrEditUserDialog: BsModalRef;
    createOrEditUserDialog = this._modalService.show(
      ThemDieuChuyenTaiSanComponent,
      {
        class: 'modal-lg',
        ignoreBackdropClick: true,
        initialState: {
          flag,
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

  deleteArrTaiSanChecked() {
    this.deleteCheckedItems();
    this.showDeleteMessage();
  }

  deleteCheckedItems() {
    this.records = this.records.filter(f =>
      !this.arrTaiSanChecked.some(s => s.id === f.id)
    );
    this.arrTaiSanChecked = [];
  }

  setControlValue(value) {
    this.form.get('PhongBan').setValue(value);
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

  getDataPage(lazyLoad?: LazyLoadEvent) {
    this.loading = true;
    // flag = 1 tài sản chưa sử dụng
    if (this.flag === 1) {
      this.records = this.taiSans;
      this.totalCount = this.records.length;
      this.loading = false;
    }

    // flag = 2 tài sản đang sử dụng
    if (this.flag === 2) {
      this.records = this.taiSans;
      this.totalCount = this.records.length;
      this.loading = false;
    }
    this.loading = false;
  }

}
