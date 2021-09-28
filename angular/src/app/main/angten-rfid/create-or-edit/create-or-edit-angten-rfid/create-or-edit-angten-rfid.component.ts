import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { AppConsts } from '@shared/AppConsts';
import { CommonComponent } from '@shared/dft/components/common.component';
import { FileDownloadService } from '@shared/file-download.service';
import {
  AngTenRFIDServiceProxy,
  CreateInputDto,
  DauDocTheRFIDServiceProxy,
  GetForViewDto,
  LookupTableDto,
  LookupTableServiceProxy,
  ThongTinHongDto,
  ThongTinHuyDto,
  ThongTinMatDto,
  ThongTinSCBDDto,
  ThongTinSDDto,
  ThongTinThanhLyDto,
  ViTriTSDto
} from '@shared/service-proxies/service-proxies';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import * as moment from 'moment';

@Component({
  selector: 'app-create-or-edit-angten-rfid',
  templateUrl: './create-or-edit-angten-rfid.component.html',
  styleUrls: ['./create-or-edit-angten-rfid.component.scss'],
  animations: [appModuleAnimation()],
})
export class CreateOrEditAngtenRfidComponent extends AppComponentBase implements OnInit {
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

  loaiTaiSanItems =
    [{
      id: 1,
      displayName: 'Đầu đọc thẻ RFID'
    },
    {
      id: 2,
      displayName: 'Đầu đọc cố định'
    }];
  loaiTaiSanValue: number;
  userDangNhap: string;
  // Upload file
  uploading = false;
  maxFile = 3;
  filesAllImg: File[] = [];
  filesAllFile: File[] = [];
  returnMessage = '';
  excelAcceptTypes = '.';
  linkServer = AppConsts.remoteServiceBaseUrl;
  createInputDto: CreateInputDto = new CreateInputDto();
  demoDto: GetForViewDto = new GetForViewDto();

  // collslap
  viTriTSList: ViTriTSDto[] = [];
  thongTinSDList: ThongTinSDDto[] = [];
  thongTinSCBDList: ThongTinSCBDDto[] = [];
  thongTinHongList: ThongTinHongDto[] = [];
  thongTinMatList: ThongTinMatDto[] = [];
  thongTinHuyList: ThongTinHuyDto[] = [];
  thongTinThanhLyList: ThongTinThanhLyDto[] = [];

  dateString = 'MM/DD/YYYY';
  ngayHienTai = moment(new Date());
  listNhaCC: LookupTableDto[];
  listLoaiTS: LookupTableDto[];

  constructor(private _fb: FormBuilder,
    injector: Injector,
    private _fileDownloadService: FileDownloadService,
    private _lookupTableService: LookupTableServiceProxy,
    private _angtenRFIDServiceProxy: AngTenRFIDServiceProxy,
    public bsModalRef: BsModalRef,
    public http: HttpClient
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.khoiTaoForm();

    this._angtenRFIDServiceProxy.getUserDangNhap().subscribe((result) => {
      this.userDangNhap = result;
    });

    this._angtenRFIDServiceProxy.getAllNhaCC().subscribe(result => {
      this.listNhaCC = result;
    });

    this._angtenRFIDServiceProxy.getAllLoaiTS().subscribe(result => {
      this.listLoaiTS = result;
    });

    if (!this.id) {
      // Thêm mới
      this.demoDto = new GetForViewDto();
      this.isEdit = false;
    } else {
      this.isEdit = true;
      // Sửa
      this._angtenRFIDServiceProxy.getForEdit(this.id, this.isView).subscribe(item => {
        this.demoDto = item;
        this._setValueForEdit();
      });
    }
    if (this.isView) {
      this.form.disable();
    } else {
      this.form.enable();
    }
    this.form.controls.loaiTaiSan.setValue(this.loaiTaiSanItems.find(w => w.id === 2));
  }

  khoiTaoForm() {
    this.form = this._fb.group({
      tenTaiSan: ['', Validators.required],
      loaiTaiSan: [],
      SerialNumber: [''],
      ProductNumber: [''],
      ReaderMACId: ['', Validators.required],
      nhaCungCap: [''],
      hangSanXuat: [''],
      nguyenGia: [''],
      ngayMua: [''],
      ngayHetHanBH: [''],
      ngayHetHanSD: [''],
      ghiChu: [''],
    });
  }
  // Bấm tải file upload
  onDownloadFile(url) {
    this._angtenRFIDServiceProxy.downloadFileUpload(url).subscribe(result => {
      this._fileDownloadService.downloadTempFile(result);
    });
  }

  searchTrangThaiDuyet(event) {
    const query = event.query;
    this._angtenRFIDServiceProxy.getAllNhaCC().subscribe(result => {
      this.suggestionsSingle = this.filterTrangThaiDuyet(query, result);
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
      this.saving = true;
      this._getValueForSave();
      this._angtenRFIDServiceProxy.createOrEdit(this.createInputDto).pipe(
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

  private _getValueForSave() {
    this.createInputDto.id = this.id;
    this.createInputDto.tenTS = this.form.controls.tenTaiSan.value;
    this.createInputDto.loaiTS = this.form.controls.loaiTaiSan.value?.id;
    this.createInputDto.serialNumber = this.form.controls.SerialNumber.value;
    this.createInputDto.productNumber = this.form.controls.ProductNumber.value;
    this.createInputDto.readerMACId = this.form.controls.ReaderMACId.value;
    this.createInputDto.nhaCC = this.form.controls.nhaCungCap.value?.id;
    this.createInputDto.hangSanXuat = this.form.controls.hangSanXuat.value;
    this.createInputDto.nguyenGia = this.form.controls.nguyenGia.value;
    this.createInputDto.ngayMua = this.form.controls.ngayMua.value;
    this.createInputDto.ngayBaoHanh = this.form.controls.ngayHetHanBH.value;
    this.createInputDto.hanSD = this.form.controls.ngayHetHanSD.value;
    this.createInputDto.ghiChu = this.form.controls.ghiChu.value;
  }

  private _setValueForEdit() {
    this.form.controls.tenTaiSan.setValue(this.demoDto.tenTS);
    this.form.controls.loaiTaiSan.setValue(this.loaiTaiSanItems.find(w => w.id === this.demoDto.loaiTS));
    this.form.controls.SerialNumber.setValue(this.demoDto.serialNumber);
    this.form.controls.ProductNumber.setValue(this.demoDto.productNumber);
    this.form.controls.ReaderMACId.setValue(this.demoDto.readerMacId);
    this.form.controls.nhaCungCap.setValue(this.listNhaCC.find(e => e.id === this.demoDto.nhaCC));
    this.form.controls.hangSanXuat.setValue(this.demoDto.hangSanXuat);
    this.form.controls.nguyenGia.setValue(this.demoDto.nguyenGia);
    this.form.controls.ngayMua.setValue(CommonComponent.getDateForEditFromMoment(this.demoDto.ngayMua));
    this.form.controls.ngayHetHanBH.setValue(CommonComponent.getDateForEditFromMoment(this.demoDto.ngayBaoHanh));
    this.form.controls.ngayHetHanSD.setValue(CommonComponent.getDateForEditFromMoment(this.demoDto.hanSD));
    this.form.controls.ghiChu.setValue(this.demoDto.ghiChu);
  }

  onSelectAll(event) {
    this.filesAll.push(...event.addedFiles);
  }

  close() {
    this.bsModalRef.hide();
  }
}
