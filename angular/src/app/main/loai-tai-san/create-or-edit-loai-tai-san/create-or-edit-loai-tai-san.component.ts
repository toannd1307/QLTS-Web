import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AppComponentBase } from '@shared/app-component-base';
import { CommonComponent } from '@shared/dft/components/common.component';
import { LoaiTaiSanCreateInputDto, LoaiTaiSanServiceProxy, LookupTableServiceProxy } from '@shared/service-proxies/service-proxies';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { finalize } from 'rxjs/operators';
@Component({
  selector: 'app-create-or-edit-loai-tai-san',
  templateUrl: './create-or-edit-loai-tai-san.component.html',
  styleUrls: ['./create-or-edit-loai-tai-san.component.scss']
})
export class CreateOrEditLoaiTaiSanComponent extends AppComponentBase implements OnInit {

  @Output() onSave = new EventEmitter<any>();
  form: FormGroup;
  saving = false;
  isEdit = false;
  id: number;
  isView = false;
  createInputDto: LoaiTaiSanCreateInputDto = new LoaiTaiSanCreateInputDto();
  loaiTaiSanItems: TreeviewItem[];

  constructor(
    injector: Injector,
    private _fb: FormBuilder,
    public bsModalRef: BsModalRef,
    private _lookupTableService: LookupTableServiceProxy,
    private _loaiTaiSanServiceProxy: LoaiTaiSanServiceProxy,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.khoiTaoForm();
    this._lookupTableService.getAllLoaiTaiSanTree().subscribe(loaiTaiSan => {
      this.loaiTaiSanItems = this.getTreeviewItem(loaiTaiSan);
      if (!this.id) {
        // Thêm mới
        this.createInputDto = new LoaiTaiSanCreateInputDto();
        this.isEdit = false;
      } else {
        this.isEdit = true;
        // Sửa
        this._loaiTaiSanServiceProxy.getForEdit(this.id, this.isView).subscribe(item => {
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
      ma: ['', Validators.required],
      ten: ['', Validators.required],
      ghiChu: [''],
    });
  }

  save(): void {
    if (CommonComponent.getControlErr(this.form) === '') {
      this.saving = true;
      this._getValueForSave();
      this._loaiTaiSanServiceProxy.createOrEdit(this.createInputDto).pipe(
        finalize(() => {
          this.saving = false;
        })
      ).subscribe((result) => {
        if (result === 1) {
          this.showExistMessage('Mã loại tài sản đã tồn tại!');
        } else if (result === 2) {
          this.showExistMessage('Tên loại tài sản đã tồn tại!');
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
    this.createInputDto.ma = this.form.controls.ma.value;
    this.createInputDto.ten = this.form.controls.ten.value;
    this.createInputDto.ghiChu = this.form.controls.ghiChu.value;
  }

  private _setValueForEdit() {
    this.form.controls.ma.setValue(this.createInputDto.ma);
    this.form.controls.ten.setValue(this.createInputDto.ten);
    this.form.controls.ghiChu.setValue(this.createInputDto.ghiChu);
  }

}
