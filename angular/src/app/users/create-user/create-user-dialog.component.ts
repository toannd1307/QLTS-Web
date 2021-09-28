// tslint:disable
import {
  Component,
  Injector,
  OnInit,
  EventEmitter,
  Output
} from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef } from 'ngx-bootstrap/modal';
import * as _ from 'lodash';
import { AppComponentBase } from '@shared/app-component-base';
import {
  UserServiceProxy,
  CreateUserDto,
  RoleDto,
  LookupTableServiceProxy
} from '@shared/service-proxies/service-proxies';
import { AbpValidationError } from '@shared/components/validation/abp-validation.api';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ValidationComponent } from '@shared/dft/components/validation-messages.component';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { CommonComponent } from '@shared/dft/components/common.component';

@Component({
  templateUrl: './create-user-dialog.component.html'
})
export class CreateUserDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  user = new CreateUserDto();
  roles: RoleDto[] = [];
  checkedRolesMap: { [key: string]: boolean } = {};
  defaultRoleCheckedStatus = false;
  passwordValidationErrors: Partial<AbpValidationError>[] = [
    {
      name: 'pattern',
      localizationKey:
        'PasswordsMustBeAtLeast8CharactersContainLowercaseUppercaseNumber',
    },
  ];
  confirmPasswordValidationErrors: Partial<AbpValidationError>[] = [
    {
      name: 'validateEqual',
      localizationKey: 'PasswordsDoNotMatch',
    },
  ];

  @Output() onSave = new EventEmitter<any>();
  form: FormGroup;
  toChucItems: TreeviewItem[];

  constructor(
    injector: Injector,
    private fb: FormBuilder,
    private _lookupTableServiceProxy: LookupTableServiceProxy,
    public _userService: UserServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.khoiTaoForm();
    this._lookupTableServiceProxy.getAllToChucTree().subscribe(toChuc => {
      this.toChucItems = this.getTreeviewItem(toChuc);
    });
    this.user.isActive = true;

    this._userService.getRoles().subscribe((result) => {
      this.roles = result.items;
      this.setInitialRolesStatus();
    });
  }

  khoiTaoForm() {
    this.form = this.fb.group({
      HoTen: ['', Validators.required],
      PhongBan: ['', Validators.required],
      ChucVu: [''],
      EmailAdress: ['', Validators.required],
      TenDangNhap: ['', Validators.required],
      SoDienThoai: [''],
      MatKhau: ['', Validators.required],
      XacNhanMatKhau: ['', Validators.required],
      TinhTrang: [true],
      GhiChu: [''],
    });

    this.form.get('MatKhau').valueChanges.subscribe(val => {
      if (val !== this.form.get('XacNhanMatKhau').value) {
        this.form.get('XacNhanMatKhau').setErrors({ validateEqual: true });
      } else if (val) {
        this.form.get('XacNhanMatKhau').setErrors(null);
      }
    });
    this.form.get('XacNhanMatKhau').valueChanges.subscribe(val => {
      if (val !== this.form.get('MatKhau').value) {
        this.form.get('XacNhanMatKhau').setErrors({ validateEqual: true });
      } else if (val) {
        this.form.get('XacNhanMatKhau').setErrors(null);
      }
    });
  }

  setControlValue(value) {
    this.form.get('PhongBan').setValue(value);
  }

  setInitialRolesStatus(): void {
    _.map(this.roles, (item) => {
      this.checkedRolesMap[item.normalizedName] = this.isRoleChecked(
        item.normalizedName
      );
    });
  }

  isRoleChecked(normalizedName: string): boolean {
    // just return default role checked status
    // it's better to use a setting
    return this.defaultRoleCheckedStatus;
  }

  onRoleChange(role: RoleDto, $event) {
    this.checkedRolesMap[role.normalizedName] = $event.target.checked;
  }

  getCheckedRoles(): string[] {
    const roles: string[] = [];
    _.forEach(this.checkedRolesMap, function (value, key) {
      if (value) {
        roles.push(key);
      }
    });
    return roles;
  }

  save(): void {
    this.user.roleNames = this.getCheckedRoles();
    if (CommonComponent.getControlErr(this.form) === '') {
      this.saving = true;
      this._getValueForSave();
      if (this.user.userName.toLocaleLowerCase() === 'admin') {
        this.showWarningMessage('Admin không được trùng!');
        this.saving = false;
      } else {
        this._userService.checkExist(this.user.userName, this.user.emailAddress, 0).subscribe((res) => {
          switch (res) {
            case 0: {
              this._userService
                .create(this.user)
                .pipe(
                  finalize(() => {
                    this.saving = false;
                  })
                )
                .subscribe(() => {
                  this.showCreateMessage();
                  this.bsModalRef.hide();
                  this.onSave.emit();
                });
              break;
            }
            case 1: {
              this.showExistMessage('Tên đăng nhập đã tồn tại!');
              this.saving = false;
              break;
            }
            case 2: {
              this.showExistMessage('Email đã tồn tại!');
              this.saving = false;
              break;
            }
            default:
              break;
          }
        });
      }
    }
  }

  private _getValueForSave() {
    this.user.name = this.form.controls.HoTen.value;
    this.user.surname = this.form.controls.HoTen.value;
    this.user.toChucId = this.form.controls.PhongBan.value;
    this.user.chucVu = this.form.controls.ChucVu.value;
    this.user.emailAddress = this.form.controls.EmailAdress.value;
    this.user.userName = this.form.controls.TenDangNhap.value;
    this.user.phoneNumber = this.form.controls.SoDienThoai.value;
    this.user.password = this.form.controls.MatKhau.value;
    this.user.isActive = this.form.controls.TinhTrang.value;
    this.user.ghiChu = this.form.controls.GhiChu.value;
  }
}
