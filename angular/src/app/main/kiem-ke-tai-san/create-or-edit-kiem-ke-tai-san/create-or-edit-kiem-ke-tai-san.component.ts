import { Component, EventEmitter, Injector, OnInit, Output, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { CommonComponent } from '@shared/dft/components/common.component';
import { StatusNumPipe } from '@shared/dft/pipes/statusnum.pipe';
import { StatusStrPipe } from '@shared/dft/pipes/statusstr.pipe';
import { KiemKeTaiSanCreateInputDto, KiemKeTaiSanServiceProxy, LookupTableServiceProxy, TrangThaiKiemKeForUpdateDto, UserForViewDto } from '@shared/service-proxies/service-proxies';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { Table } from 'primeng/table';
import { finalize } from 'rxjs/operators';
import { ThemNguoiKiemKeTaiSanComponent } from './them-nguoi-kiem-ke-tai-san/them-nguoi-kiem-ke-tai-san.component';
import { DanhSachKiemKeTaiSanComponent } from './danh-sach-kiem-ke-tai-san/danh-sach-kiem-ke-tai-san.component';

@Component({
  selector: 'app-create-or-edit-kiem-ke-tai-san',
  templateUrl: './create-or-edit-kiem-ke-tai-san.component.html',
  styleUrls: ['./create-or-edit-kiem-ke-tai-san.component.scss'],
  animations: [appModuleAnimation()]
})
export class CreateOrEditKiemKeTaiSanComponent extends AppComponentBase implements OnInit {

  @Output() onSave = new EventEmitter<any>();
  @ViewChild('dt') table: Table;
  loading = true;
  form: FormGroup;
  saving = false;
  isEdit = false;
  id: number;
  isView = false;
  trangThaiID: number;
  finish = false;
  createInputDto: KiemKeTaiSanCreateInputDto = new KiemKeTaiSanCreateInputDto();
  kiemKeDto: TrangThaiKiemKeForUpdateDto = new TrangThaiKiemKeForUpdateDto();
  toChucItems: TreeviewItem[];
  toChucValue: number; // Bộ phận được kiểm kê
  records: UserForViewDto[] = []; // Người kiểm kê
  totalCount = 0;
  selectedRows: UserForViewDto[];

  constructor(
    injector: Injector,
    private _fb: FormBuilder,
    public bsModalRef: BsModalRef,
    private _modalService: BsModalService,
    private _kiemKeTaiSanServiceProxy: KiemKeTaiSanServiceProxy,
    private _lookupTableService: LookupTableServiceProxy,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.khoiTaoForm();
    this._lookupTableService.getAllToChucTheoNguoiDungTree(true).subscribe((toChuc) =>
      this.toChucItems = this.getTreeviewItem(toChuc)
    );
    this.finish = this.trangThaiID === 0 || this.trangThaiID === 2;
    if (!this.id) {
      // Thêm mới
      this.createInputDto = new KiemKeTaiSanCreateInputDto();
      this.isEdit = false;
      this.loading = false;
    } else {
      this.isEdit = true;
      // Sửa
      this._kiemKeTaiSanServiceProxy.getForEdit(this.id, this.isView).subscribe(item => {
        this.createInputDto = item;
        this._setValueForEdit();
      });
      // lấy người kiểm kê theo id
      this._kiemKeTaiSanServiceProxy.getUserForEdit(this.id).pipe(finalize(() => { this.loading = false; })).subscribe(item => {
        this.records = item;
      });
    }
    // Xem
    if (this.isView) {
      this.form.disable();
    } else {
      this.form.enable();
    }
  }
  // check xóa nhiều
  isDeletes() {
    if (!this.selectedRows || this.selectedRows.length === 0) {
      return true;
    }
    return false;
  }

  deleteAll() {
    this.records = this.records.filter(value => !this.selectedRows.includes(value));
    this.selectedRows = [];
  }

  isToChucDisable() {
    if (this.isView) {
      return true;
    } else {
      return false;
    }
  }

  khoiTaoForm() {
    this.form = this._fb.group({
      maKiemKe: ['', Validators.required],
      tenKiemKe: ['', Validators.required],
      predictTimeStart: ['', Validators.required],
      predictTimeEnd: ['', Validators.required],
      realTimeStart: [''],
      realTimeEnd: [''],
      department: ['', Validators.required],
      status: ['Chưa bắt đầu'],
      doiKiemKeIdList: []
    });
    this.form.get('predictTimeStart').valueChanges.subscribe(val => {
      if (val > this.form.get('predictTimeEnd').value && this.form.get('predictTimeEnd').value) {
        this.form.get('predictTimeStart').setErrors({ isStartMaxLdk: true });
        this.form.get('predictTimeStart').markAsDirty({ onlySelf: true });
      } else if (val) {
        this.form.get('predictTimeStart').setErrors(null);
        this.form.get('predictTimeEnd').setErrors(null);
      }
    });
    this.form.get('predictTimeEnd').valueChanges.subscribe(val => {
      if (val < this.form.get('predictTimeStart').value && this.form.get('predictTimeStart').value) {
        this.form.get('predictTimeEnd').setErrors({ isEndMinLdk: true });
        this.form.get('predictTimeEnd').markAsDirty({ onlySelf: true });
      } else if (val) {
        this.form.get('predictTimeEnd').setErrors(null);
        this.form.get('predictTimeStart').setErrors(null);
      }
    });
  }

  setControlValue(value) {
    this.form.get('department').setValue(value);
  }

  save(): void {
    const filterPipe = new StatusStrPipe();
    this._getValueForSave();
    this.createInputDto.trangThaiId = filterPipe.transform(this.createInputDto.trangThaiId); // cập nhật trạng thái ở view
    if (this.isView) {
      this.ketThucKiemke();
    } else if (CommonComponent.getControlErr(this.form) === '') {
        if (this.records.length > 0) {
          this.saving = true;
          this._kiemKeTaiSanServiceProxy.createOrEdit(this.createInputDto).pipe(
            finalize(() => {
              this.saving = false;
            })
          ).subscribe((result) => {
            if (result === 1) {
              this.showExistMessage('Mã kiểm kê đã bị trùng!');
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
        } else {
          this.showErrorMessage('Chọn ít nhất một người kiểm kê');
        }
      }
  }

  ketThucKiemke() {
    const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
    '<p class="text-popup-xoa m-t-8">'
     + 'Đợt kiểm kê sẽ kết thúc!' + '</p>';
    this.swal.fire({
      html: html1,
      icon: 'warning',
      iconHtml: '<span class="icon1">&#9888</span>',
      showCancelButton: true,
      confirmButtonColor: this.confirmButtonColor,
      cancelButtonColor: this.cancelButtonColor,
      cancelButtonText: this.cancelButtonText,
      confirmButtonText: 'Đồng ý'
    }).then((result) => {
      if (result.value) {
        this.kiemKeDto.id = this.id;
        this.kiemKeDto.trangThai = false;
        this._kiemKeTaiSanServiceProxy.updateTrangThaiKiemKe(this.kiemKeDto).pipe(
          finalize(() => {
            this.saving = false;
          })
        ).subscribe(() => {
          this.bsModalRef.hide();
          this.onSave.emit();
        });
      }
    });
  }

  close() {
    this.bsModalRef.hide();
  }
  private _showAddNguoiKiemKe(): void {
    const userSave = this.records;
    let addUserDialog: BsModalRef;
    addUserDialog = this._modalService.show(
      ThemNguoiKiemKeTaiSanComponent,
      {
        class: 'modal-xl',
        ignoreBackdropClick: true,
        initialState: {
          userSave
        },
      }
    );
    // ouput emit
    addUserDialog.content.event.subscribe((data) => {
      data.forEach(e => {
        this.records.unshift(e);
      });
    });
  }
  isModalTrue() {
    const formValues = this.form.controls.status.value;
    if (CommonComponent.getControlErr(this.form) !== '') {
      return true;
    } else if (formValues === 'Chưa bắt đầu' || formValues === 'Đã kết thúc' && this.isView === true) {
      return true;
    } else {
      return false;
    }
  }

  addPerson() {
    this._showAddNguoiKiemKe();
  }

  private _getValueForSave() {
    this.createInputDto.maKiemKe = this.form.controls.maKiemKe.value;
    this.createInputDto.tenKiemKe = this.form.controls.tenKiemKe.value;
    this.createInputDto.thoiGianBatDauDuKien = this.form.controls.predictTimeStart.value;
    this.createInputDto.thoiGianKetThucDuKien = this.form.controls.predictTimeEnd.value;
    this.createInputDto.boPhanDuocKiemKeId = this.toChucValue;
    this.createInputDto.trangThaiId = this.form.controls.status.value;
    this.createInputDto.doiKiemKeIdList = this.records.map(e => e.user.id);
  }

  private _setValueForEdit() {
    const filterPipe = new StatusNumPipe();
    this.form.controls.maKiemKe.setValue(this.createInputDto.maKiemKe);
    this.form.controls.tenKiemKe.setValue(this.createInputDto.tenKiemKe);
    this.form.controls.predictTimeStart.setValue(CommonComponent.getDateForEditFromMoment(this.createInputDto.thoiGianBatDauDuKien));
    this.form.controls.realTimeStart.setValue(CommonComponent.getDateForEditFromMoment(this.createInputDto.thoiGianBatDauThucTe));
    this.form.controls.predictTimeEnd.setValue(CommonComponent.getDateForEditFromMoment(this.createInputDto.thoiGianKetThucDuKien));
    this.form.controls.realTimeEnd.setValue(CommonComponent.getDateForEditFromMoment(this.createInputDto.thoiGianKetThucThucTe));
    this.toChucValue = this.createInputDto.boPhanDuocKiemKeId;
    this.form.controls.department.setValue(this.createInputDto.boPhanDuocKiemKeId);
    this.form.controls.status.setValue(filterPipe.transform(this.createInputDto.trangThaiId));
    this.form.controls.doiKiemKeIdList.setValue(this.createInputDto.doiKiemKeIdList);
  }

}
