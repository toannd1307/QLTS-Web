import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AppComponentBase } from '@shared/app-component-base';
import { AppConsts } from '@shared/AppConsts';
import { CommonComponent } from '@shared/dft/components/common.component';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { FileDownloadService } from '@shared/file-download.service';
import {
  CreateDuTruInput,
  LookupTableDto,
  LookupTableServiceProxy,
  PhieuDuTruMuaSamChiTiet,
  PhieuDuTruMuaSamDinhKemFile,
  PhieuDuTruMuaSamServiceProxy
} from '@shared/service-proxies/service-proxies';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { PhieuDuTruMuaSamComponent } from './phieu-du-tru-mua-sam/phieu-du-tru-mua-sam.component';
const URL = AppConsts.remoteServiceBaseUrl + '/api/Upload/DuTruMuaSamUpload';

@Component({
  selector: 'app-create-or-edit-du-tru-mua-sam',
  templateUrl: './create-or-edit-du-tru-mua-sam.component.html',
  styleUrls: ['./create-or-edit-du-tru-mua-sam.component.scss']
})
export class CreateOrEditDuTruMuaSamComponent extends AppComponentBase implements OnInit {
  @Output() onSave = new EventEmitter<any>();
  form: FormGroup;
  isActive = false;
  loading = true;
  saving = false;
  isEdit = false;
  id: number;
  isView = false;
  suggestionsSingle: LookupTableDto[];
  filesAll: File[] = [];

  listIdXoa: number[] = [];
  selectedList: PhieuDuTruMuaSamChiTiet[] = [];
  // Upload file
  uploading = false;
  maxFile = 3;
  filesAllImg: File[] = [];
  filesAllFile: File[] = [];
  listFileAnh: PhieuDuTruMuaSamDinhKemFile[] = [];
  listFileWord: PhieuDuTruMuaSamDinhKemFile[] = [];
  linkServer = AppConsts.remoteServiceBaseUrl;
  createInputDto: CreateDuTruInput = new CreateDuTruInput();
  toChucItems: TreeviewItem[];
  records: PhieuDuTruMuaSamChiTiet[] = [];
  phieuDuTruInput: PhieuDuTruMuaSamChiTiet;
  totalCount = 0;
  demoDto: CreateDuTruInput = new CreateDuTruInput();
  titleNotice = 'Bạn chắc chắn không?';
  dateString = 'MM/DD/YYYY';

  constructor(private _fb: FormBuilder,
    injector: Injector,
    private _modalService: BsModalService,
    private _fileDownloadService: FileDownloadService,
    private _lookupTableService: LookupTableServiceProxy,
    private _duTruMuaSamServiceProxy: PhieuDuTruMuaSamServiceProxy,
    public bsModalRef: BsModalRef,
    public http: HttpClient
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.khoiTaoForm();

    this._lookupTableService.getAllToChucTheoNguoiDungTree(true).subscribe(toChuc => {
      this.toChucItems = this.getTreeviewItem(toChuc);
    });


    if (!this.id) {
      // Thêm mới
      this.demoDto = new CreateDuTruInput();
      this.isEdit = false;
    } else {
      this.isEdit = true;
      // Sửa
      this._duTruMuaSamServiceProxy.getForEdit(this.id, this.isView).subscribe(item => {
        this.demoDto = item;
        this._setValueForEdit();
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
      maPhieu: ['', Validators.required],
      tenPhieu: ['', Validators.required],
      phongBan: ['', Validators.required],
      nguoiLapPhieu: [this.appSession.user.name, Validators.required],
    });
  }

  setControlValue(value) {
    this.form.get('phongBan').setValue(value);
  }

  xoaList(res: PhieuDuTruMuaSamChiTiet[]) {

    res.forEach(w => {
      const index: number = this.records.indexOf(w);
      if (index !== -1) {
        this.records.splice(index, 1);
      }
    });
    this.selectedList = [];
  }

  onSelectAllHA(event) {
    if (!this.isView) {
      this.filesAllImg.push(...event.addedFiles);
    }
  }

  onSelectAllFile(event) {
    this.filesAllFile.push(...event.addedFiles);
  }
  onRemoveAllHA(event) {
    this.filesAllImg.splice(this.filesAllImg.indexOf(event), 1);
  }

  onRemoveAllFile(event) {
    this.filesAllFile.splice(this.filesAllFile.indexOf(event), 1);
  }

  onDownloadFile(url) {
    this._duTruMuaSamServiceProxy.downloadFileUpload(url).subscribe(result => {
      this._fileDownloadService.downloadTempFile(result);
    });
  }

  xoaListHA() {
    this.filesAllImg = [];
  }

  xoaListFile() {
    this.filesAllFile = [];

  }

  create(id?: number) {
    this._showCreateOrEditDemoDialog(id);
  }

  private _showCreateOrEditDemoDialog(id?: number, isView = false): void {
    // copy
    let createOrEditUserDialog: BsModalRef;
    createOrEditUserDialog = this._modalService.show(
      PhieuDuTruMuaSamComponent,
      {
        class: 'modal-xl',
        ignoreBackdropClick: true,
        initialState: {
          id,
          isView,
        },
      }
    );

    // ouput emit
    createOrEditUserDialog.content.onSave.subscribe((input) => {
      this.records.push(input);
    });
  }

  filterTrangThaiDuyet(query, trangThai: LookupTableDto[]): any[] {
    const filtered: any[] = [];
    for (const iterator of trangThai) {
      if (iterator.displayName.toLowerCase().indexOf(query.toLowerCase()) === 0) {
        filtered.push(iterator);
      }
    }
    return filtered;
  }

  save() {
    if (CommonComponent.getControlErr(this.form) === '') {

      const formdata = new FormData();

      for (let i = 0; i < this.filesAllImg.length; i++) {
        const item = new File([this.filesAllImg[i]], this.filesAllImg[i].name);
        formdata.append((i + 1) + '', item);
      }

      for (let i = 0; i < this.filesAllFile.length; i++) {
        const item = new File([this.filesAllFile[i]], this.filesAllFile[i].name);
        formdata.append((i + 1) + '', item);
      }

      this.http.post(URL, formdata).subscribe((res) => {
        this.createInputDto.listDinhKem = [];
        this.xuLyFile(res);

        this.saving = true;
        this._getValueForSave();
        this._duTruMuaSamServiceProxy.createOrEdit(this.createInputDto).pipe(
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
      });
    }
  }

  xuLyFile(res) {
    for (const file of this.filesAllImg) {
      const item = new PhieuDuTruMuaSamDinhKemFile();
      item.tenFile = file.name;
      item.phanLoaiId = 1;
      item.linkFile = '\\' + res['result'][res['result']
        .findIndex(e => e.includes(file.name))].split('\\').slice(-2).join('\\');
      this.createInputDto.listDinhKem.push(item);
    }

    for (const file of this.filesAllFile) {
      const item = new PhieuDuTruMuaSamDinhKemFile();
      item.tenFile = file.name;
      item.phanLoaiId = 0;
      item.linkFile = '\\' + res['result'][res['result']
        .findIndex(e => e.includes(file.name))].split('\\').slice(-2).join('\\');
      this.createInputDto.listDinhKem.push(item);
    }
  }


  private _getValueForSave() {
    this.createInputDto.id = this.id;
    this.createInputDto.maPhieu = this.form.controls.maPhieu.value;
    this.createInputDto.tenPhieu = this.form.controls.tenPhieu.value;
    this.createInputDto.toChucId = this.form.controls.phongBan.value;
    this.createInputDto.nguoiLapPhieuId = this.appSession.userId;
    this.createInputDto.listPhieuChiTiet = this.records;
  }

  private _setValueForEdit() {
    this.form.controls.maPhieu.setValue(this.demoDto.maPhieu);
    this.form.controls.tenPhieu.setValue(this.demoDto.tenPhieu);
    this.form.controls.phongBan.setValue(this.demoDto.toChucId);
    this._duTruMuaSamServiceProxy.getUserDangNhap(this.demoDto.nguoiLapPhieuId).subscribe(w => {
      this.form.controls.nguoiLapPhieu.setValue(w);
    });
    this.records = this.demoDto.listPhieuChiTiet;
    if (this.isView) {
      this.form.disable();
    }
    // hiển thị file
    for (const file of this.demoDto.listDinhKem) {
      const path =
        AppConsts.remoteServiceBaseUrl +
        '\\Upload\\DuTruMuaSam' +
        file.linkFile;
      this.http.get(path, { responseType: 'blob' }).subscribe((data) => {
        if (file.phanLoaiId === 1) {
          this.filesAllImg.push(this.blobToFile(data, file.tenFile));
          this.listFileAnh.push(file);
        } else {
          this.filesAllFile.push(this.blobToFile(data, file.tenFile));
          this.listFileWord.push(file);
        }
      });
    }
  }

  onSelectAll(event) {
    this.filesAll.push(...event.addedFiles);
  }

  close() {
    this.bsModalRef.hide();
  }
}
