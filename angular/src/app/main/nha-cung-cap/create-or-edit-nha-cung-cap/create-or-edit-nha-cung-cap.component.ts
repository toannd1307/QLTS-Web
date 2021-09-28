import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AppComponentBase } from '@shared/app-component-base';
import { AppConsts } from '@shared/AppConsts';
import { CommonComponent } from '@shared/dft/components/common.component';
import { spaceValidator, ValidationComponent } from '@shared/dft/components/validation-messages.component';
import { FileDownloadService } from '@shared/file-download.service';
import { LookupTableDto, LookupTableServiceProxy, NhaCungCapCreateInputDto, NhaCungCapServiceProxy, NhaCungCap_File } from '@shared/service-proxies/service-proxies';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
const URL = AppConsts.remoteServiceBaseUrl + '/api/Upload/NhaCungCapUpload';
@Component({
  selector: 'app-create-or-edit-nha-cung-cap',
  templateUrl: './create-or-edit-nha-cung-cap.component.html',
})
export class CreateOrEditNhaCungCapComponent extends AppComponentBase implements OnInit {

  @Output() onSave = new EventEmitter<any>();
  form: FormGroup;
  saving = false;
  isEdit = false;
  uploading = false;
  id: number;
  isView = false;
  listLinhVuc: LookupTableDto[];
  filesAllFile: File[] = [];
  createInputDto: NhaCungCapCreateInputDto = new NhaCungCapCreateInputDto();

  constructor(
    injector: Injector,
    private _fb: FormBuilder,
    public bsModalRef: BsModalRef,
    private _nhaCungCapServiceProxy: NhaCungCapServiceProxy,
    private _lookupTableServiceProxy: LookupTableServiceProxy,
    private _fileDownloadService: FileDownloadService,
    public http: HttpClient,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.khoiTaoForm();
    forkJoin(
      this._lookupTableServiceProxy.getAllLinhVucKinhDoanh(),
    ).subscribe(([linhVuc]) => {
      this.listLinhVuc = linhVuc;
      if (!this.id) {
        // Thêm mới
        this.createInputDto = new NhaCungCapCreateInputDto();
        this.isEdit = false;
      } else {
        this.isEdit = true;
        // Sửa
        this._nhaCungCapServiceProxy.getForEdit(this.id, this.isView).subscribe(item => {
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
      maNhaCungCap: ['', [Validators.required, ValidationComponent.KtraChuThuong]],
      tenNhaCungCap: ['', Validators.required],
      linhVuc: [],
      maSoThue: [''],
      diaChi: [''],
      soDienThoai: [''],
      email: ['', Validators.email],
      ghiChu: [''],
    });
  }

  save(): void {
    if (CommonComponent.getControlErr(this.form) === '') {
      const formdata = new FormData();
      for (let i = 0; i < this.filesAllFile.length; i++) {
        formdata.append((i + 1) + '', this.filesAllFile[i]);
      }

      this.http.post(URL, formdata).subscribe((res) => {
        this._getValueForSave();
        this.createInputDto.listFile = [];
        this.xuLyFile(res);


        this.saving = true;
        this._getValueForSave();
        this._nhaCungCapServiceProxy.createOrEdit(this.createInputDto).pipe(
          finalize(() => {
            this.saving = false;
          })
        ).subscribe((result) => {
          if (result === 1) {
            this.showExistMessage('Mã nhà cung cấp đã bị trùng!');
          } else if (result === 2) {
            this.showExistMessage('Tên nhà cung cấp đã bị trùng!');
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
      });
    }
  }

  xuLyFile(res) {

    for (const file of this.filesAllFile) {
      const item = new NhaCungCap_File();
      item.tenFile = file.name;
      item.linkFile = '\\' + res['result'][res['result']
        .findIndex(e => e.includes(file.name))].split('\\').slice(-2).join('\\');
      this.createInputDto.listFile.push(item);
    }
  }

  uppercaseAll(e) {
    this.form.controls.maNhaCungCap.setValue(CommonComponent.vietThuong(e));
  }

  onSelectAllFile(event) {
    this.filesAllFile.push(...event.addedFiles);
  }

  onRemoveAllFile(event) {
    this.filesAllFile.splice(this.filesAllFile.indexOf(event), 1);
  }

  onDownloadFile(url) {
    this._nhaCungCapServiceProxy.downloadFileUpload(url).subscribe(result => {
      this._fileDownloadService.downloadTempFile(result);
    });
  }

  close() {
    this.bsModalRef.hide();
  }

  private _getValueForSave() {
    this.createInputDto.maNhaCungCap = this.form.controls.maNhaCungCap.value;
    this.createInputDto.tenNhaCungCap = this.form.controls.tenNhaCungCap.value;
    this.createInputDto.diaChi = this.form.controls.diaChi.value;
    this.createInputDto.linhVucKinhDoanhId = this.form.controls.linhVuc.value?.id;
    this.createInputDto.maSoThue = this.form.controls.maSoThue.value;
    this.createInputDto.soDienThoai = this.form.controls.soDienThoai.value;
    this.createInputDto.email = this.form.controls.email.value;
    this.createInputDto.ghiChu = this.form.controls.ghiChu.value;
  }

  private _setValueForEdit() {
    this.form.controls.maNhaCungCap.setValue(this.createInputDto.maNhaCungCap);
    this.form.controls.tenNhaCungCap.setValue(this.createInputDto.tenNhaCungCap);
    this.form.controls.diaChi.setValue(this.createInputDto.diaChi);
    this.form.controls.maSoThue.setValue(this.createInputDto.maSoThue);
    this.form.controls.linhVuc.setValue(this.listLinhVuc.find(e => e.id === this.createInputDto.linhVucKinhDoanhId));
    this.form.controls.soDienThoai.setValue(this.createInputDto.soDienThoai);
    this.form.controls.email.setValue(this.createInputDto.email);
    this.form.controls.ghiChu.setValue(this.createInputDto.ghiChu);

    for (const file of this.createInputDto.listFile) {
      const path =
        AppConsts.remoteServiceBaseUrl +
        '\\Upload\\NhaCungCap' +
        file.linkFile;
      this.http.get(path, { responseType: 'blob' }).subscribe((data) => {
        const fileDto = new File([data], file.tenFile);
        this.filesAllFile.push(fileDto);
      });
    }
  }

}
