import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { CommonComponent } from '@shared/dft/components/common.component';
import { CreateOrEditDtos, GetValueForView, LookupTableDto, ViTriDiaLyServiceProxy } from '@shared/service-proxies/service-proxies';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-create-or-edit-vitri-dialy',
  templateUrl: './create-or-edit-vitri-dialy.component.html',
  styleUrls: ['./create-or-edit-vitri-dialy.component.scss'],
  animations: [appModuleAnimation()],

})
export class CreateOrEditVitriDialyComponent extends AppComponentBase implements OnInit {

  @Output() onSave = new EventEmitter<any>();
  form: FormGroup;

  tt: LookupTableDto[];
  ttValue: number;
  qh: LookupTableDto[];
  qhFromTT: LookupTableDto[];
  qhValue: number;

  demoDto: GetValueForView = new GetValueForView();
  id: number;
  createInputDto: CreateOrEditDtos = new CreateOrEditDtos();
  saving = false;
  isEdit = false;
  isView = false;

  constructor(private _fb: FormBuilder,
    injector: Injector,
    private _viTriDiaLyServiceProxy: ViTriDiaLyServiceProxy,
    public bsModalRef: BsModalRef) {
    super(injector);
  }

  ngOnInit(): void {

    this._viTriDiaLyServiceProxy.getAllDtoTinhThanh().subscribe((result) => {
      this.tt = result;
    });

    this.khoiTaoForm();

    if (!this.id) {
      // Thêm mới
      this.demoDto = new GetValueForView();
      this.isEdit = false;
    } else {
      this.isEdit = true;
      // Sửa
      forkJoin(this._viTriDiaLyServiceProxy.getAllDtoQuanHuyen(),
        this._viTriDiaLyServiceProxy.getForEdit(this.id, this.isView),
      ).subscribe(([rs, rs1]) => {
        this.demoDto = rs1;
        this.qh = rs;
        this._viTriDiaLyServiceProxy.getAllDtoQuanHuyenFromTT(this.demoDto.tinhThanh).subscribe((result) => {
          this.qhFromTT = result;
          this._setValueForEdit();
        });
      });
    }
    if (this.isView) {
      this.form.disable();
    } else {
      this.form.enable();
    }

  }

  khoiTaoForm() {
    this.form = this._fb.group({
      tenViTri: ['', Validators.required],
      tinhThanh: ['', Validators.required],
      quanHuyen: ['', Validators.required],
      diaChi: ['', Validators.required],
      ghiChu: [''],
    });
  }


  save() {

    if (CommonComponent.getControlErr(this.form) === '') {
      this.saving = true;
      this._getValueForSave();
      this._viTriDiaLyServiceProxy.createOrEdit(this.createInputDto).pipe(
        finalize(() => {
          this.saving = false;
        })
      ).subscribe((result) => {
        if (!this.id) {
          if (result === -1) {
            this.showExistMessage('Tên vị trí đã tồn tại!');
          } else {
            this.showCreateMessage();
            this.bsModalRef.hide();
            this.onSave.emit();
          }
        } else if (result === -1) {
          this.showExistMessage('Tên vị trí đã tồn tại!');
        } else {
          this.bsModalRef.hide();
          this.onSave.emit();
          this.showUpdateMessage();
        }
      });
    }
  }

  private _getValueForSave() {
    this.createInputDto.id = this.id;
    this.createInputDto.tenViTri = this.form.controls.tenViTri.value;
    this.createInputDto.tinhThanh = this.form.controls.tinhThanh.value?.id;
    this.createInputDto.quanHuyen = this.form.controls.quanHuyen.value?.id;
    this.createInputDto.diaChi = this.form.controls.diaChi.value;
    this.createInputDto.ghiChu = this.form.controls.ghiChu.value;
  }

  getQuanHuyen() {
    this._viTriDiaLyServiceProxy.getAllDtoQuanHuyen().subscribe((result) => {
      this.qh = result;
    });
  }

  private _setValueForEdit() {
    this.form.controls.tenViTri.setValue(this.demoDto.tenViTri);
    this.form.controls.tinhThanh.setValue(this.tt.find(e => e.id === this.demoDto.tinhThanh));
    this.form.controls.quanHuyen.setValue(this.qhFromTT.find(e => e.id === this.demoDto.quanHuyen));
    this.form.controls.diaChi.setValue(this.demoDto.diaChi);
    this.form.controls.ghiChu.setValue(this.demoDto.ghiChu);
  }

  getlistQH() {
    this._viTriDiaLyServiceProxy.getAllDtoQuanHuyenFromTT(this.form.controls.tinhThanh.value?.id).subscribe((result) => {
      this.qhFromTT = result;
    });
  }

}

