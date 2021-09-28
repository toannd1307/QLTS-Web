// tslint:disable
import {
  Component,
  Injector,
  OnInit,
  EventEmitter,
  Output,
} from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef } from 'ngx-bootstrap/modal';
import * as _ from 'lodash';
import { AppComponentBase } from '@shared/app-component-base';
import {
  RoleServiceProxy,
  GetRoleForEditOutput,
  RoleDto,
  PermissionDto,
  RoleEditDto,
  FlatPermissionDto
} from '@shared/service-proxies/service-proxies';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { CommonComponent } from '@shared/dft/components/common.component';

@Component({
  templateUrl: 'edit-role-dialog.component.html'
})
export class EditRoleDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  isPermissionActive = false;
  id: number;
  role = new RoleEditDto();
  permissions: FlatPermissionDto[];
  grantedPermissionNames: string[];
  checkedPermissionsMap: { [key: string]: boolean } = {};

  @Output() onSave = new EventEmitter<any>();
  form: FormGroup;

  constructor(
    injector: Injector,
    private fb: FormBuilder,
    private _roleService: RoleServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.khoiTaoForm();
    this._roleService
      .getRoleForEdit(this.id)
      .subscribe((result: GetRoleForEditOutput) => {
        this.role = result.role;
        this.permissions = result.permissions;
        this.grantedPermissionNames = result.grantedPermissionNames;
        this._setValueForEdit();
        this.setInitialPermissionsStatus();
      });
  }

  khoiTaoForm() {
    this.form = this.fb.group({
      TenVaiTro: ['', Validators.required],
      TenHienThi: ['', Validators.required],
      GhiChu: [''],
    });
  }

  setInitialPermissionsStatus(): void {
    _.map(this.permissions, (item) => {
      this.checkedPermissionsMap[item.name] = this.isPermissionChecked(
        item.name
      );
    });
  }

  isPermissionChecked(permissionName: string): boolean {
    return _.includes(this.grantedPermissionNames, permissionName);
  }

  onPermissionChange(permission: PermissionDto, $event) {
    this.checkedPermissionsMap[permission.name] = $event.target.checked;
  }

  getCheckedPermissions(): string[] {
    const permissions: string[] = [];
    _.forEach(this.checkedPermissionsMap, function (value, key) {
      if (value) {
        permissions.push(key);
      }
    });
    return permissions;
  }

  save(): void {
    if (CommonComponent.getControlErr(this.form) === '') {
      this.saving = true;
      this._getValueForSave();
      this._roleService.checkExist(this.role.name, this.role.displayName, this.role.id).subscribe((res) => {
        switch (res) {
          case 0: {
            const role = new RoleDto();
            role.init(this.role);
            role.grantedPermissions = this.getCheckedPermissions();

            this._roleService
              .update(role)
              .pipe(
                finalize(() => {
                  this.saving = false;
                })
              )
              .subscribe(() => {
                this.showUpdateMessage();
                this.bsModalRef.hide();
                this.onSave.emit();
              });
            break;
          }
          case 1: {
            this.showExistMessage('Tên vai trò đã tồn tại!');
            this.saving = false;
            break;
          }
          case 2: {
            this.showExistMessage('Tên hiển thị đã tồn tại!');
            this.saving = false;
            break;
          }
          default:
            break;
        }
      });
    }
  }

  private _setValueForEdit() {
    this.form.controls.TenVaiTro.setValue(this.role.name);
    this.form.controls.TenHienThi.setValue(this.role.displayName);
    this.form.controls.GhiChu.setValue(this.role.description);
  }

  private _getValueForSave() {
    this.role.name = this.form.controls.TenVaiTro.value;
    this.role.displayName = this.form.controls.TenHienThi.value;
    this.role.description = this.form.controls.GhiChu.value;
  }
}
