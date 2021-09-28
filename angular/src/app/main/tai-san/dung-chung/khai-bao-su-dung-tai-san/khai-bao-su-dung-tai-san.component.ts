import {
  LookupTableDto, LookupTableServiceProxy, PhieuTaiSanChiTietDto, PhieuTaiSanCreateInputDto,
  TaiSanChuaSuDungForViewDto, TaiSanChuaSuDungServiceProxy
} from '@shared/service-proxies/service-proxies';
import { Table } from 'primeng/table';
import { AppComponentBase } from '@shared/app-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Component, EventEmitter, Injector, OnInit, Output, ViewChild } from '@angular/core';
import { LazyLoadEvent } from 'primeng/api';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { forkJoin } from 'rxjs';
import { CommonComponent } from '@shared/dft/components/common.component';
import { spaceValidator } from '@shared/dft/components/validation-messages.component';
import { finalize } from 'rxjs/operators';
import { ThemKhaiBaoSuDungTaiSanComponent } from './them-khai-bao-su-dung-tai-san/them-khai-bao-su-dung-tai-san.component';

@Component({
  selector: 'app-khai-bao-su-dung-tai-san',
  templateUrl: './khai-bao-su-dung-tai-san.component.html',
  styleUrls: ['./khai-bao-su-dung-tai-san.component.scss']
})
export class KhaiBaoSuDungTaiSanComponent extends AppComponentBase implements OnInit {

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

  constructor(
    injector: Injector,
    private _fb: FormBuilder,
    public bsModalRef: BsModalRef,
    private _lookupTableService: LookupTableServiceProxy,
    private _modalService: BsModalService,
    private _taiSanChuaSuDungServiceProxy: TaiSanChuaSuDungServiceProxy
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.khoiTaoForm();
    forkJoin(
      this._lookupTableService.getAllToChucTheoNguoiDungTree(true),
    ).subscribe(([toChuc]) => {
      this.toChucItems = this.getTreeviewItem(toChuc);
    });
  }

  khoiTaoForm(): void {
    this.form = this._fb.group({
      thoiGianKhaiBao: [new Date(), Validators.required],
      noiDungKhaiBao: ['', [Validators.maxLength(4000), spaceValidator(), Validators.required]],
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
          this.showSuccessMessage('Khai báo thành công');
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
      phanLoaiId: this.enums.TrangThaiTaiSanConst.TaiSanDangSuDung,
      ngayKhaiBao: this.form.value.thoiGianKhaiBao,
      noiDung: this.form.value.noiDungKhaiBao,
      phieuTaiSanChiTietList: items
    };
  }

  close(): void {
    this.bsModalRef.hide();
  }

  deleteTaiSan(): void {
  }

  addTaiSan(): void {
    this._showCreateOrEditDemoDialog(1);
  }

  private _showCreateOrEditDemoDialog(flag: number): void {
    let createOrEditUserDialog: BsModalRef;
    createOrEditUserDialog = this._modalService.show(
      ThemKhaiBaoSuDungTaiSanComponent,
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

    }
    this.loading = false;
  }
}
