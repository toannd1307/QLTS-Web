import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AppComponentBase } from '@shared/app-component-base';
import { CommonComponent } from '@shared/dft/components/common.component';
import { PhieuDuTruMuaSamChiTiet } from '@shared/service-proxies/service-proxies';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-phieu-du-tru-mua-sam',
  templateUrl: './phieu-du-tru-mua-sam.component.html',
  styleUrls: ['./phieu-du-tru-mua-sam.component.scss']
})
export class PhieuDuTruMuaSamComponent extends AppComponentBase implements OnInit {
  @Output() onSave = new EventEmitter<any>();
  form: FormGroup;
  isView = false;
  saving = false;
  constructor(private _fb: FormBuilder,
    injector: Injector,
    public bsModalRef: BsModalRef,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.khoiTaoForm();
  }

  khoiTaoForm() {
    this.form = this._fb.group({
      tenTaiSan: ['', Validators.required],
      productNumber: ['', Validators.required],
      hangSanXuat: ['', Validators.required],
      nhaCungCap: ['', Validators.required],
      soLuong: ['', Validators.required],
      donGia: ['', Validators.required],
      mucDichSuDung: ['', Validators.required],
    });
  }
  save() {
    if (CommonComponent.getControlErr(this.form) === '') {
    const input = new PhieuDuTruMuaSamChiTiet();
    input.tenTaiSan = this.form.controls.tenTaiSan.value;
    input.productNumber = this.form.controls.productNumber.value;
    input.hangSanXuat = this.form.controls.hangSanXuat.value;
    input.nhaCungCap = this.form.controls.nhaCungCap.value;
    input.soLuong = this.form.controls.soLuong.value;
    input.donGia = this.form.controls.donGia.value;
    input.ghiChu = this.form.controls.mucDichSuDung.value;
    this.onSave.emit(input);
    this.showCreateMessage();
    this.close();
    }
  }

  close() {
    this.bsModalRef.hide();
  }

}
