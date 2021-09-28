import { Component, EventEmitter, Injector, OnInit, Output, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AppComponentBase } from '@shared/app-component-base';
import { CommonComponent } from '@shared/dft/components/common.component';
import {
  LookupTableDto,
  LookupTableServiceProxy,
  MailServerServiceProxy,
  PhieuTaiSanCreateInputDto,
  TaiSanThanhLyServiceProxy,
  ViewTaiSanSuaChuaBaoDuong as ViewTaiSanThanhLy,
} from '@shared/service-proxies/service-proxies';
import { finalize, map } from 'rxjs/operators';
import { Table } from 'primeng/table';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { PhieuTaiSanChiTietDto } from './../../../../../shared/service-proxies/service-proxies';
import { spaceValidator } from '@shared/dft/components/validation-messages.component';
import { ThemTaiSanThanhLyComponent } from './../them-tai-san-thanh-ly/them-tai-san-thanh-ly.component';
import { appModuleAnimation } from '@shared/animations/routerTransition';

@Component({
  selector: 'app-create-or-edit-tai-san-thanh-ly',
  templateUrl: './create-or-edit-tai-san-thanh-ly.component.html',
  styleUrls: ['./create-or-edit-tai-san-thanh-ly.component.scss'],
  animations: [appModuleAnimation()],
})
export class CreateOrEditTaiSanThanhLyComponent extends AppComponentBase implements OnInit {

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

  arrTaiSanChecked: ViewTaiSanThanhLy[] = [];


  constructor(
    injector: Injector,
    private _modalService: BsModalService,
    private _taiSanServiceProxy: TaiSanThanhLyServiceProxy,
    private _lookupTableService: LookupTableServiceProxy,
    private _fb: FormBuilder,
    public bsModalRef: BsModalRef,
    private _sendMail: MailServerServiceProxy
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.khoiTaoForm();
  }

  create(id?: number) {
    this._showCreateOrEditDemoDialog(id);
  }

  khoiTaoForm() {
    this.form = this._fb.group({
      nguoiKhaiBao: [this.appSession.user.name, Validators.required],
      thoiGianKhaiBao: [new Date(), Validators.required],
      noiDungKhaiBao: ['', [Validators.required, Validators.maxLength(4000), spaceValidator()]],
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
      ThemTaiSanThanhLyComponent,
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
      this._taiSanServiceProxy.createTaiSanThanhLy(this.createInputDto).pipe(
        finalize(() => {
          this.saving = false;
        })
      ).subscribe((result) => {
        if (result === 1) {
          this.showExistMessage('Lỗi');
        } else if (!this.id) {
          this.showCreateMessage();
          this._sendMail.sendMailThanhLy(undefined, this.records).subscribe(rs => {});
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
      items.push(item);
    });
    this.createInputDto = {
      ...this.form.value,
      phanLoaiId: this.enums.TrangThaiTaiSanConst.TaiSanThanhLy,
      ngayKhaiBao: this.form.value.thoiGianKhaiBao,
      noiDung: this.form.value.noiDungKhaiBao,
      phieuTaiSanChiTietList: items
    };
  }
}
