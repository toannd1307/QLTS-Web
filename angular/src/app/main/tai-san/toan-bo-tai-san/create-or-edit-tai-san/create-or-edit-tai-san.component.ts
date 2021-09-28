import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { AppConsts } from '@shared/AppConsts';
import { CommonComponent } from '@shared/dft/components/common.component';
import { FileDownloadService } from '@shared/file-download.service';
import {
  CreateInputDto,
  GetForViewDto,
  LookupTableDto,
  LookupTableServiceProxy,
  TaiSanDinhKemFile,
  ThongTinHongDto,
  ThongTinHuyDto,
  ThongTinMatDto, ThongTinSCBDDto, ThongTinSDDto, ThongTinThanhLyDto, ToanBoTaiSanServiceProxy, ViTriTSDto
} from '@shared/service-proxies/service-proxies';
import * as moment from 'moment';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { finalize } from 'rxjs/operators';
const URL = AppConsts.remoteServiceBaseUrl + '/api/Upload/ToanBoTSUpload';
@Component({
  selector: 'app-create-or-edit-tai-san',
  templateUrl: './create-or-edit-tai-san.component.html',
  styleUrls: ['./create-or-edit-tai-san.component.scss'],
  animations: [appModuleAnimation()],
})
export class CreateOrEditTaiSanComponent extends AppComponentBase implements OnInit {
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

  idTSCreated: number;

  loaiTaiSanItems: TreeviewItem[];
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
  demoFile: CreateInputDto = new CreateInputDto();

  // collslap
  viTriTSList: ViTriTSDto[] = [];
  thongTinSDList: ThongTinSDDto[] = [];
  thongTinSCBDList: ThongTinSCBDDto[] = [];
  thongTinHongList: ThongTinHongDto[] = [];
  thongTinMatList: ThongTinMatDto[] = [];
  thongTinHuyList: ThongTinHuyDto[] = [];
  thongTinThanhLyList: ThongTinThanhLyDto[] = [];

  totalCountVTTS = 0;
  totalCountTTSD = 0;
  totalCountSCBD = 0;
  totalCountHong = 0;
  totalCountMat = 0;
  totalCountHuy = 0;
  totalCountTL = 0;

  tinhTrangRFID = '';
  tinhTrangQRCode = '';
  tinhTrangBarCode = '';

  tinhTrangRFIDbool: boolean;
  tinhTrangQRCodebool: boolean;
  tinhTrangBarCodebool: boolean;

  maSD: LookupTableDto[];
  dateString = 'MM/DD/YYYY';
  ngayHienTai = moment(new Date());
  listNhaCC: LookupTableDto[];
  listLoaiTS: LookupTableDto[];
  arrNguonKinhPhi: LookupTableDto[];
  constructor(private _fb: FormBuilder,
    injector: Injector,
    private _fileDownloadService: FileDownloadService,
    private _lookupTableService: LookupTableServiceProxy,
    private _toanBoTaiSanServiceProxy: ToanBoTaiSanServiceProxy,
    public bsModalRef: BsModalRef,
    public http: HttpClient
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.khoiTaoForm();

    this._lookupTableService.getAllLoaiTaiSanTree().subscribe((loaiTaiSan) => {
      loaiTaiSan = loaiTaiSan.filter(e => e.value !== 1 && e.value !== 2);
      this.loaiTaiSanItems = this.getTreeviewItem(loaiTaiSan);
    });

    this._toanBoTaiSanServiceProxy.getUserDangNhap().subscribe((result) => {
      this.userDangNhap = result;
    });

    this._lookupTableService.getAllNguonKinhPhi().subscribe((result) => {
      this.arrNguonKinhPhi = result;
    });

    this._toanBoTaiSanServiceProxy.getAllNhaCC().subscribe(result => {
      this.listNhaCC = result;
    });

    this._toanBoTaiSanServiceProxy.getAllLoaiTS().subscribe(result => {
      this.listLoaiTS = result;
    });
    this._lookupTableService.getAllTinhTrangMaSuDungTaiSan().subscribe(result => {
      this.maSD = result;
    });

    this.form.controls.ngayMua.valueChanges.subscribe((val: Date) => {
      const soNamKhauHao = this.form.controls.trichKH.value;
      if (soNamKhauHao && val && val.toString() !== '') {
        const ngayHetHan = new Date(val.getFullYear() + soNamKhauHao, val.getMonth(), val.getDate());
        this.form.controls.hetKH.setValue(ngayHetHan);
      } else {
        this.form.controls.hetKH.setValue('');
      }
    });

    this.form.controls.trichKH.valueChanges.subscribe((nam: number) => {
      const ngayMua = this.form.controls.ngayMua.value;
      if (nam && ngayMua && ngayMua.toString() !== '') {
        const ngayHetHan = new Date(ngayMua.getFullYear() + nam, ngayMua.getMonth(), ngayMua.getDate());
        this.form.controls.hetKH.setValue(ngayHetHan);
      } else {
        this.form.controls.hetKH.setValue('');
      }
    });

    if (!this.id) {
      // Thêm mới
      this.demoDto = new GetForViewDto();
      this.isEdit = false;
    } else {
      this.isEdit = true;
      // Sửa
      this._toanBoTaiSanServiceProxy.getForEdit(this.id, this.isView).subscribe(item => {
        this.demoDto = item;
        console.log(118, item);
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
      tenTaiSan: ['', Validators.required],
      loaiTaiSan: ['', Validators.required],
      SerialNumber: [''],
      ProductNumber: [''],
      nhaCungCap: [''],
      nguonKinhPhi: [''],
      hangSanXuat: [''],
      nguyenGia: [''],
      ngayMua: [''],
      ngayHetHanBH: [''],
      ngayHetHanSD: [''],
      ghiChu: [''],
      giaCuoiTS: [''],
      noiDungChotGia: [''],
      nguoiChotGia: [''],
      thoiDiemChotGia: [''],
      maRFID: [''],
      maBarCode: [''],
      maQRCode: [''],
      maEPCCode: [''],
      maQuanLy: [''],
      DropdownMultiple: [],
      trichKH: [''],
      hetKH: [''],
    });
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


  // Bấm tải file upload
  onDownloadFile(url) {
    this._toanBoTaiSanServiceProxy.downloadFileUpload(url).subscribe(result => {
      this._fileDownloadService.downloadTempFile(result);
    });
  }

  searchTrangThaiDuyet(event) {
    const query = event.query;
    this._toanBoTaiSanServiceProxy.getAllNhaCC().subscribe(result => {
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

  setControlValue(value) {
    this.loaiTaiSanValue = value;
    this.form.get('loaiTaiSan').setValue(value);
  }

  save() {
    if (CommonComponent.getControlErr(this.form) === '') {
      this.saving = true;
      const formdata = new FormData();
      formdata.append('tenTS', this.form.controls.tenTaiSan.value);
      for (let i = 0; i < this.filesAllImg.length; i++) {
        const itemmm = new File([this.filesAllImg[i]], this.filesAllImg[i].name);
        formdata.append((i + 1) + '', itemmm);
      }

      for (let i = 0; i < this.filesAllFile.length; i++) {
        const itemmm = new File([this.filesAllFile[i]], this.filesAllFile[i].name);
        formdata.append((i + 1) + '', itemmm);
      }

      this.http.post(URL, formdata).subscribe((res) => {
        this._getValueForSave();
        this.createInputDto.listHA = [];
        this.createInputDto.listFile = [];
        this.xuLyFile(res);
        this._getValueForSave();
        this._toanBoTaiSanServiceProxy.createOrEdit(this.createInputDto).pipe(
          finalize(() => {
            this.saving = false;
          })
        ).subscribe((result) => {
          this.idTSCreated = result;
          if (!this.id) {
            this.showCreateMessage();
            this.bsModalRef.hide();
            this.onSave.emit(this.idTSCreated);
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
      const item = new TaiSanDinhKemFile();
      item.tenFile = file.name;
      item.linkFile = this.getLinkFile(res, file.name);
      this.createInputDto.listHA.push(item);
    }

    for (const file of this.filesAllFile) {
      const item = new TaiSanDinhKemFile();
      item.tenFile = file.name;
      item.linkFile = this.getLinkFile(res, file.name);
      this.createInputDto.listFile.push(item);
    }
  }

  private _getValueForSave() {
    this.createInputDto.id = this.id;
    this.createInputDto.tenTS = this.form.controls.tenTaiSan.value;
    this.createInputDto.loaiTS = this.loaiTaiSanValue;
    this.createInputDto.serialNumber = this.form.controls.SerialNumber.value;
    this.createInputDto.productNumber = this.form.controls.ProductNumber.value;
    this.createInputDto.nhaCC = this.form.controls.nhaCungCap.value?.id;
    this.createInputDto.nguonKinhPhiId = this.form.controls.nguonKinhPhi.value?.id;
    this.createInputDto.hangSanXuat = this.form.controls.hangSanXuat.value;
    this.createInputDto.nguyenGia = this.form.controls.nguyenGia.value;
    this.createInputDto.ngayMua = this.form.controls.ngayMua.value;
    this.createInputDto.ngayBaoHanh = this.form.controls.ngayHetHanBH.value;
    this.createInputDto.hanSD = this.form.controls.ngayHetHanSD.value;
    this.createInputDto.ghiChu = this.form.controls.ghiChu.value;
    this.createInputDto.giaCuoiTS = this.form.controls.giaCuoiTS.value;
    this.createInputDto.noiDungChotGia = this.form.controls.noiDungChotGia.value;
    this.createInputDto.thoiGianChietKhauHao = this.form.controls.trichKH.value;
    if (this.form.controls.giaCuoiTS.value !== '') {
      this.createInputDto.thoiDiemChotGia = this.ngayHienTai;
      this.createInputDto.nguoiChotGia = this.appSession.userId;
    } else {
      this.createInputDto.thoiDiemChotGia = undefined;
      this.createInputDto.nguoiChotGia = undefined;
    }
    this.createInputDto.dropdownMultiple = this.form.controls.DropdownMultiple.value ?
      this.form.controls.DropdownMultiple.value?.map(e => e.id).join(this.separator) : null;
  }

  private _setValueForEdit() {
    this.form.controls.tenTaiSan.setValue(this.demoDto.tenTS);
    this.loaiTaiSanValue = this.demoDto.loaiTS;
    this.form.controls.loaiTaiSan.setValue(this.demoDto.loaiTS);

    this.form.controls.SerialNumber.setValue(this.demoDto.serialNumber);
    this.form.controls.ProductNumber.setValue(this.demoDto.productNumber);
    this.form.controls.nhaCungCap.setValue(this.listNhaCC.find(e => e.id === this.demoDto.nhaCC));
    this.form.controls.nguonKinhPhi.setValue(this.arrNguonKinhPhi.find(e => e.id === this.demoDto.nguonKinhPhiId));
    this.form.controls.hangSanXuat.setValue(this.demoDto.hangSanXuat);
    this.form.controls.nguyenGia.setValue(this.demoDto.nguyenGia);
    this.form.controls.ngayMua.setValue(CommonComponent.getDateForEditFromMoment(this.demoDto.ngayMua));
    this.form.controls.ngayHetHanBH.setValue(CommonComponent.getDateForEditFromMoment(this.demoDto.ngayBaoHanh));
    this.form.controls.ngayHetHanSD.setValue(CommonComponent.getDateForEditFromMoment(this.demoDto.hanSD));
    this.form.controls.ghiChu.setValue(this.demoDto.ghiChu);
    this.form.controls.noiDungChotGia.setValue(this.demoDto.noiDungChotGia);
    this.form.controls.thoiDiemChotGia.setValue(this.demoDto.thoiDiemChotGia != null ? this.demoDto.thoiDiemChotGia.format('DD/MM/YYYY') : '');
    this.form.controls.nguoiChotGia.setValue(this.demoDto.nguoiChotGia);
    this.form.controls.giaCuoiTS.setValue(this.demoDto.giaCuoiTS);
    this.form.controls.maRFID.setValue(this.demoDto.maRFID);
    this.form.controls.maBarCode.setValue(this.demoDto.maBarCode);
    this.form.controls.maQRCode.setValue(this.demoDto.maQRCode);
    this.form.controls.maEPCCode.setValue(this.demoDto.epcCode);
    this.form.controls.maQuanLy.setValue(this.demoDto.maQuanLy);
    this.form.controls.trichKH.setValue(this.demoDto.thoiGianChietKhauHao);
    this.tinhTrangRFID = this.demoDto.tinhTrangMaRFID;
    this.tinhTrangQRCode = this.demoDto.tinhTrangMaQRCode;
    this.tinhTrangBarCode = this.demoDto.tinhTrangMaBarCode;

    this.tinhTrangRFIDbool = this.demoDto.tinhTrangRFID;
    this.tinhTrangBarCodebool = this.demoDto.tinhTrangBarCode;
    this.tinhTrangQRCodebool = this.demoDto.tinhTrangQRCode;
    const listDropdownMultiple = this.demoDto.dropdownMultiple?.split(this.separator).map(e => Number(e));
    this.form.controls.DropdownMultiple.setValue(listDropdownMultiple ?
      this.maSD.filter(e => listDropdownMultiple.findIndex(w => w === e.id) > -1) : null);

    // hiển thị file
    for (const file of this.demoDto.listHinhAnh) {
      const path =
        AppConsts.remoteServiceBaseUrl +
        '\\Upload\\ToanBoTS' +
        file.linkFile;
      this.http.get(path, { responseType: 'blob' }).subscribe((data) => {
        this.filesAllImg.push(this.blobToFile(data, file.tenFile));
      });
    }

    for (const file of this.demoDto.listFile) {
      const path =
        AppConsts.remoteServiceBaseUrl +
        '\\Upload\\ToanBoTS' +
        file.linkFile;
      this.http.get(path, { responseType: 'blob' }).subscribe((data) => {
        this.filesAllFile.push(this.blobToFile(data, file.tenFile));
      });
    }

    this.viTriTSList = this.demoDto.listViTriTS;
    this.totalCountVTTS = this.demoDto.listViTriTS.length;
    this.thongTinSDList = this.demoDto.listThongTinSD;
    this.totalCountTTSD = this.demoDto.listThongTinSD.length;
    this.thongTinSCBDList = this.demoDto.listThongTinSCBD;
    this.totalCountSCBD = this.demoDto.listThongTinSCBD.length;
    this.thongTinHongList = this.demoDto.listThongTinHong;
    this.totalCountHong = this.demoDto.listThongTinHong.length;
    this.thongTinHuyList = this.demoDto.listThongTinHuy;
    this.totalCountHuy = this.demoDto.listThongTinHuy.length;
    this.thongTinMatList = this.demoDto.listThongTinMat;
    this.totalCountMat = this.demoDto.listThongTinMat.length;
    this.thongTinThanhLyList = this.demoDto.listThongTinThanhLy;
    this.totalCountTL = this.demoDto.listThongTinThanhLy.length;

  }

  changeGiaCuoiTS(event) {
    if (event !== undefined) {
      this.form.controls.thoiDiemChotGia.setValue(this.ngayHienTai.format('DD/MM/YYYY'));
      this.form.controls.nguoiChotGia.setValue(this.appSession.user.name);
    }
  }

  xoaListHA() {
    this.filesAllImg = [];
  }

  xoaListFile() {
    this.filesAllFile = [];

  }

  onSelectAll(event) {
    this.filesAll.push(...event.addedFiles);
  }

  close() {
    this.bsModalRef.hide();
  }
}
