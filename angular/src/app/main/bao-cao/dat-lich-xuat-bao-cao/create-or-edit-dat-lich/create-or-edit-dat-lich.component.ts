import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AppComponentBase } from '@shared/app-component-base';
import { CommonComponent } from '@shared/dft/components/common.component';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { CreateOrEditDatLichDtos, DatLichXuatBaoCaoServiceProxy, GetValueForViewDatLich, LookupTableDto, LookupTableServiceProxy, } from '@shared/service-proxies/service-proxies';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
import * as moment from 'moment';
@Component({
  selector: 'app-create-or-edit-dat-lich',
  templateUrl: './create-or-edit-dat-lich.component.html',
  styleUrls: ['./create-or-edit-dat-lich.component.scss']
})
export class CreateOrEditDatLichComponent extends AppComponentBase implements OnInit {
  [x: string]: any;
  @Output() onSave = new EventEmitter<any>();
  form: FormGroup;
  saving = false;
  isEdit = false;
  uploading = false;
  id: number;
  isView = false;
  listBaoCao: LookupTableDto[];
  listLapLai: LookupTableDto[];
  listThang: LookupTableDto[];
  listUsers: LookupTableDto[];
  filesAllFile: File[] = [];
  createInputDto: CreateOrEditDatLichDtos = new CreateOrEditDatLichDtos();
  toChucItems: TreeviewItem[];
  getForView: GetValueForViewDatLich = new GetValueForViewDatLich();
  toChucValue: number;
  listToChucId: number[];
  check: number;
  phongBanValueId: number;
  nguoiNhanValueId: number;
  constructor(
    injector: Injector,
    private _fb: FormBuilder,
    public bsModalRef: BsModalRef,
    private _datLichServiceProxy: DatLichXuatBaoCaoServiceProxy,
    private _lookupTableServiceProxy: LookupTableServiceProxy,
    public http: HttpClient,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.khoiTaoForm();
    this.changeLapLai();
    this.changePB();
    forkJoin(
      this._lookupTableServiceProxy.getAllBaoCaoLookupTable(),
      this._lookupTableServiceProxy.getAllLapLaiLookupTable(),
      this._lookupTableServiceProxy.getAllToChucTree(),
    ).subscribe(([baoCao, lapLai, toChuc]) => {
      this.listBaoCao = baoCao;
      this.listLapLai = lapLai;
      this.toChucItems = this.getTreeviewItem(toChuc);
      if (!this.id) {
        // Thêm mới
        this.createInputDto = new CreateOrEditDatLichDtos();
        this.isEdit = false;
      } else {
        this.isEdit = true;
        // Sửa
        this._datLichServiceProxy.getForEdit(this.id, this.isView).subscribe(item => {
          this.getForView = item;
          this._setValueForEdit();
        });
      }
      if (this.isView) {
        this.form.disable();
      } else {
        this.form.enable();
      }
    });
  }

  khoiTaoForm() {
    this.form = this._fb.group({
      tenBaoCao: ['', [Validators.required]],
      lapLai: ['', [Validators.required]],
      gioGuiBC: ['', Validators.required],
      nguoiNhanBC: ['', Validators.required],
      phongBanNhanBC: ['', Validators.required],
      ngayGuiBC: ['', Validators.required],
      ghiChu: [''],
    });
  }

  changeLapLai() {
    this.form.controls.lapLai.valueChanges.subscribe(w => {
      this.check = w.id;
      this.form.controls.ngayGuiBC.setValue(undefined);
      if (w !== undefined && w.id === 1) {
        this._lookupTableServiceProxy.getAllThuLookupTable().subscribe(e => {
          this.listThang = e;
        });
      } else if (w !== undefined && w.id === 2) {
        this._lookupTableServiceProxy.getAllThangLookupTable().subscribe(e => {
          this.listThang = e;
        });
      }
      this.setEror(w.id);
    });
  }
  setEror(id: number) {
    if (id === 0 || id === 3) {
      this.form.controls.ngayGuiBC.setErrors(null);
    } else {
      this.form.controls.ngayGuiBC.setValidators([Validators.required]);
    }
  }

  changePB() {
    this.form.controls.phongBanNhanBC.valueChanges.subscribe(w => {
      if (w !== undefined) {
        this._lookupTableServiceProxy.getAllNguoiDungTheoPBLookupTable(w).subscribe(e => {
          this.listUsers = e;
        });
      } else {
        this.form.controls.nguoiNhanBC.setValue('');
      }
    });
  }

  save(): void {
    if (CommonComponent.getControlErr(this.form) === '') {
      this.saving = true;
      this._getValueForSave();
      this._datLichServiceProxy.createOrEdit(this.createInputDto).pipe(
        finalize(() => {
          this.saving = false;
        })
      ).subscribe((result) => {
        if (!this.id) {
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
    this.createInputDto.id = this.id;
    this.createInputDto.baoCaoId = this.form.controls.tenBaoCao.value?.id;
    this.createInputDto.lapLaiId = this.form.controls.lapLai.value?.id;
    this.createInputDto.gioGuiBaoCao = this.form.controls.gioGuiBC.value;

    this.createInputDto.nguoiNhanBaoCaoId = this.form.controls.nguoiNhanBC.value ?
      this.form.controls.nguoiNhanBC.value?.map(e => e.id).join(this.separator) : null;

    this.createInputDto.phongBanNhanId = this.form.controls.phongBanNhanBC.value;
    if (this.form.controls.lapLai.value?.id === 4) {
      this.createInputDto.ngayGuiBaoCao = moment(this.form.controls.ngayGuiBC.value.toString()).format('DD/MM');
    } else {
      this.createInputDto.ngayGuiBaoCao = this.form.controls.ngayGuiBC.value?.id;
    }
    this.createInputDto.ghiChu = this.form.controls.ghiChu.value;
  }

  private _setValueForEdit() {
    this.check = this.getForView.lapLaiId;
    this.form.controls.tenBaoCao.setValue(this.listBaoCao.find(e => e.id === this.getForView.baoCaoId));
    this.form.controls.lapLai.setValue(this.listLapLai.find(e => e.id === this.getForView.lapLaiId));
    this.form.controls.gioGuiBC.setValue(CommonComponent.getDateForEditFromMoment(this.getForView.gioBaoCao));
    this.phongBanValueId = this.getForView.phongBanNhan;
    this.form.controls.phongBanNhanBC.setValue(this.getForView.phongBanNhan);

    this._lookupTableServiceProxy.getAllNguoiDungTheoPBLookupTable(this.getForView.phongBanNhan
    ).subscribe(result => {
      this.listUsers = result;
    }, () => { }, () => {
      const listDropdownMultiple = this.getForView.nguoiNhan?.split(this.separator).map(e => Number(e));
      this.form.controls.nguoiNhanBC.setValue(listDropdownMultiple ?
        this.listUsers.filter(e => listDropdownMultiple.findIndex(w => w === e.id) > -1) : null);
    });

    if (this.check === 1) {
      this._lookupTableServiceProxy.getAllThuLookupTable().subscribe(e => {
        this.listThang = e;
      }, () => { }, () => {
        this.form.controls.ngayGuiBC.setValue(this.listThang.find(e => e.id === Number(this.getForView.ngayGuiBaoCao)));
      });
    } else if (this.check === 2) {
      this._lookupTableServiceProxy.getAllThangLookupTable().subscribe(e => {
        this.listThang = e;
      }, () => { }, () => {
        this.form.controls.ngayGuiBC.setValue(this.listThang.find(e => e.id === Number(this.getForView.ngayGuiBaoCao)));
      });
    } else if (this.check === 4) {
      const namHienTai = moment().year();
      const ngayGuiBaoCao = `${namHienTai}/ ${this.getForView.ngayGuiBaoCao.split('/').reverse().join('/')}`;
      this.form.controls.ngayGuiBC.setValue(CommonComponent.getDateForEditFromString(ngayGuiBaoCao));
    }

    this.form.controls.ghiChu.setValue(this.getForView.ghiChu);
  }
  setControlValue(value) {
    this.form.get('phongBanNhanBC').setValue(value);
  }
}
