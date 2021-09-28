import { LookupTableDto } from './../../../../shared/service-proxies/service-proxies';
import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AppComponentBase } from '@shared/app-component-base';
import { CommonComponent } from '@shared/dft/components/common.component';
import { LookupTableServiceProxy, ToChucCreateInputDto, ToChucServiceProxy } from '@shared/service-proxies/service-proxies';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
@Component({
  selector: 'app-create-or-edit-to-chuc',
  templateUrl: './create-or-edit-to-chuc.component.html',
  styleUrls: ['./create-or-edit-to-chuc.component.scss']
})
export class CreateOrEditToChucComponent extends AppComponentBase implements OnInit {

  @Output() onSave = new EventEmitter<any>();
  form: FormGroup;
  saving = false;
  isEdit = false;
  id: number;
  isView = false;
  createInputDto: ToChucCreateInputDto = new ToChucCreateInputDto();
  toChucItems: LookupTableDto[];
  viTriDiaLyItems: LookupTableDto[];

  constructor(
    injector: Injector,
    private _fb: FormBuilder,
    public bsModalRef: BsModalRef,
    private _lookupTableService: LookupTableServiceProxy,
    private _toChucServiceProxy: ToChucServiceProxy,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.khoiTaoForm();
    forkJoin(
      this._toChucServiceProxy.getAllToChucCha(),
      this._lookupTableService.getAllViTriDiaLyLookupTable(),
    ).subscribe(([toChuc, viTriDiaLy]) => {
      this.toChucItems = toChuc;
      this.viTriDiaLyItems = viTriDiaLy;
      if (!this.id) {
        // Thêm mới
        this.createInputDto = new ToChucCreateInputDto();
        this.isEdit = false;
      } else {
        this.isEdit = true;
        // Sửa
        this._toChucServiceProxy.getForEdit(this.id, this.isView).subscribe(item => {
          this.createInputDto = item;
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
      maToChuc: ['', Validators.required],
      tenToChuc: ['', Validators.required],
      viTriDiaLyId: ['', Validators.required],
      trucThuocToChucId: [undefined, Validators.required],
      maHexa: [''],
      ghiChu: [''],
    });
  }

  save(): void {
    if (CommonComponent.getControlErr(this.form) === '') {
      this.saving = true;
      this._getValueForSave();
      this._toChucServiceProxy.createOrEdit(this.createInputDto).pipe(
        finalize(() => {
          this.saving = false;
        })
      ).subscribe((result) => {
        if (result === 1) {
          this.showExistMessage('Mã đơn vị đã tồn tại!');
        } else if (result === 2) {
          this.showExistMessage('Tên đơn vị đã tồn tại!');
        } else if (result === 3) {
          this.showExistMessage('Mã hexa đã tồn tại!');
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
    this.createInputDto.maToChuc = this.form.controls.maToChuc.value;
    this.createInputDto.tenToChuc = this.form.controls.tenToChuc.value;

    // Nếu là thêm mới thì mới gán
    if (!this.id) {
      this.createInputDto.trucThuocToChucId = this.form.controls.trucThuocToChucId.value?.id;
    }
    this.createInputDto.viTriDiaLyId = this.form.controls.viTriDiaLyId.value?.id;
    this.createInputDto.ghiChu = this.form.controls.ghiChu.value;
  }

  private _setValueForEdit() {
    this.form.controls.maToChuc.setValue(this.createInputDto.maToChuc);
    this.form.controls.tenToChuc.setValue(this.createInputDto.tenToChuc);
    this.form.controls.maHexa.setValue(this.createInputDto.maHexa);
    this.form.controls.maHexa.disable();
    this.form.controls.trucThuocToChucId.setValue(this.toChucItems.find(e => e.id === this.createInputDto.trucThuocToChucId));
    this.form.controls.viTriDiaLyId.setValue(this.viTriDiaLyItems.find(e => e.id === this.createInputDto.viTriDiaLyId));
    this.form.controls.ghiChu.setValue(this.createInputDto.ghiChu);

    this.form.controls.trucThuocToChucId.disable();
    // Nếu là phòng ban cha
    if (!this.createInputDto.trucThuocToChucId) {
      this.form.controls.trucThuocToChucId.setErrors(null);
    }
  }

}
