import { Component, Injector, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { CommonComponent } from '@shared/dft/components/common.component';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { LookupTableServiceProxy, MailServerDto, MailServerServiceProxy } from '@shared/service-proxies/service-proxies';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-cau-hinh-mail-server',
  templateUrl: './cau-hinh-mail-server.component.html',
  styleUrls: ['./cau-hinh-mail-server.component.scss'],
  animations: [appModuleAnimation()]
})
export class CauHinhMailServerComponent extends AppComponentBase implements OnInit {

  form: FormGroup;
  saving = false;
  isEdit = false;
  id: number;
  isView = false;
  createInputDto: MailServerDto = new MailServerDto();
  loaiTaiSanItems: TreeviewItem[];

  constructor(
    injector: Injector,
    private _fb: FormBuilder,
    private _mailServerServiceProxy: MailServerServiceProxy,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.khoiTaoForm();
    this._mailServerServiceProxy.getForEdit().subscribe(item => {
      this.createInputDto = item;
      if (item !== undefined) {
        this._setValueForEdit();
      }
    });
  }

  khoiTaoForm() {
    this.form = this._fb.group({
      host: ['', Validators.required],
      port: ['', Validators.required],
      email: ['', Validators.required],
      password: ['', Validators.required],
      Cbx_CapPhat: [],
      Cbx_ThuHoi: [],
      Cbx_DieuChuyen: [],
      Cbx_BaoMat: [],
      Cbx_BaoHong: [],
      Cbx_ThanhLy: [],
      Cbx_SuaChuaBaoDuong: [],
      Cbx_BatDauKiemKe: [],
      Cbx_KetThucKiemKe: [],
      Cbx_HoanThanhPhieuDTMS: [],
      Cbx_HuyBoPhieuDTMS: [],
    });
  }

  save(): void {
    if (CommonComponent.getControlErr(this.form) === '') {
      this.saving = true;
      this._getValueForSave();
      this._mailServerServiceProxy.updateMailServer(this.createInputDto).pipe(
        finalize(() => {
          this.saving = false;
        })
      ).subscribe((result) => {
        if (result === 0) {
          this.showCreateMessage();
        } else {
          this.showUpdateMessage();
        }
        this.ngOnInit();
      });
    }
  }

  private _getValueForSave() {
    this.createInputDto.host = this.form.controls.host.value;
    this.createInputDto.port = this.form.controls.port.value;
    this.createInputDto.email = this.form.controls.email.value;
    this.createInputDto.password = this.form.controls.password.value;
    this.createInputDto.capPhat = this.form.controls.Cbx_CapPhat.value;
    this.createInputDto.thuHoi = this.form.controls.Cbx_ThuHoi.value;
    this.createInputDto.dieuChuyen = this.form.controls.Cbx_DieuChuyen.value;
    this.createInputDto.baoMat = this.form.controls.Cbx_BaoMat.value;
    this.createInputDto.baoHong = this.form.controls.Cbx_BaoHong.value;
    this.createInputDto.thanhLy = this.form.controls.Cbx_ThanhLy.value;
    this.createInputDto.suaChuaBaoDuong = this.form.controls.Cbx_SuaChuaBaoDuong.value;
    this.createInputDto.batDauKiemKe = this.form.controls.Cbx_BatDauKiemKe.value;
    this.createInputDto.ketThucKiemKe = this.form.controls.Cbx_KetThucKiemKe.value;
    this.createInputDto.hoanThanhPhieuDuTruMuaSam = this.form.controls.Cbx_HoanThanhPhieuDTMS.value;
    this.createInputDto.huyBoPhieuDuTruMuaSam = this.form.controls.Cbx_HuyBoPhieuDTMS.value;
  }

  private _setValueForEdit() {
    this.form.controls.host.setValue(this.createInputDto.host);
    this.form.controls.port.setValue(this.createInputDto.port);
    this.form.controls.email.setValue(this.createInputDto.email);
    this.form.controls.password.setValue(this.createInputDto.password);
    this.form.controls.Cbx_CapPhat.setValue(this.createInputDto.capPhat);
    this.form.controls.Cbx_ThuHoi.setValue(this.createInputDto.thuHoi);
    this.form.controls.Cbx_DieuChuyen.setValue(this.createInputDto.dieuChuyen);
    this.form.controls.Cbx_BaoMat.setValue(this.createInputDto.baoMat);
    this.form.controls.Cbx_BaoHong.setValue(this.createInputDto.baoHong);
    this.form.controls.Cbx_ThanhLy.setValue(this.createInputDto.thanhLy);
    this.form.controls.Cbx_SuaChuaBaoDuong.setValue(this.createInputDto.suaChuaBaoDuong);
    this.form.controls.Cbx_BatDauKiemKe.setValue(this.createInputDto.batDauKiemKe);
    this.form.controls.Cbx_KetThucKiemKe.setValue(this.createInputDto.ketThucKiemKe);
    this.form.controls.Cbx_HoanThanhPhieuDTMS.setValue(this.createInputDto.hoanThanhPhieuDuTruMuaSam);
    this.form.controls.Cbx_HuyBoPhieuDTMS.setValue(this.createInputDto.huyBoPhieuDuTruMuaSam);
  }

}
