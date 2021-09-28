import { LookupTableDto, TreeviewItemDto } from './../../shared/service-proxies/service-proxies';
import { LazyLoadEvent } from 'primeng/api';
import { OnInit, ViewChild } from '@angular/core';
// tslint:disable
import { Component, Injector } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  PagedListingComponentBase,
  PagedRequestDto
} from 'shared/paged-listing-component-base';
import {
  UserServiceProxy,
  UserDto,
  UserDtoPagedResultDto,
  LookupTableServiceProxy
} from '@shared/service-proxies/service-proxies';
import { CreateUserDialogComponent } from './create-user/create-user-dialog.component';
import { EditUserDialogComponent } from './edit-user/edit-user-dialog.component';
import { ResetPasswordDialogComponent } from './reset-password/reset-password.component';
import { AppComponentBase } from '@shared/app-component-base';
import { Table } from 'primeng/table';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { PermissionTreeEditModel } from '@shared/dft/components/permission-tree-edit.model';
@Component({
  templateUrl: './users.component.html',
  animations: [appModuleAnimation()]
})
export class UsersComponent extends AppComponentBase implements OnInit {
  @ViewChild('dt') table: Table;
  users: UserDto[] = [];
  keyword = '';
  isActive: boolean | null;
  advancedFiltersVisible = false;
  loading = true;
  totalCount = 0;
  toChucs: LookupTableDto[] = [];
  toChucValue: number[];
  toChucTrees: PermissionTreeEditModel;
  first = 0;
  constructor(
    injector: Injector,
    private _userService: UserServiceProxy,
    private _lookupTableServiceProxy: LookupTableServiceProxy,
    private _modalService: BsModalService
  ) {
    super(injector);
  }
  ngOnInit(): void {
    this._userService.getAllDonVi().subscribe(result => {
      this.toChucs = result;
    });
    this._lookupTableServiceProxy.getAllToChucCuaNguoiDangNhapTree().subscribe((data) => {
      this.toChucTrees = {
        data,
        selectedData: data.map(e => e.id),
      };
      this.toChucValue = data.map(e => e.id);
       this.getDataPage(false);
    });
  }

  getDataPage(isSearch:boolean, lazyLoad?: LazyLoadEvent) {
    this.loading = true;
    this._userService
      .getAll(
        this.keyword || undefined,
        this.isActive || undefined,
        this.toChucValue || undefined,
        isSearch,
        this.getSortField(this.table),
        lazyLoad ? lazyLoad.first : this.first,
        lazyLoad ? lazyLoad.rows : this.table.rows,
      ).subscribe((result: UserDtoPagedResultDto) => {
        this.loading = false;
        this.users = result.items;
        this.totalCount = result.totalCount;
      });
  }

  getToChuc(toChucId: number) {
    return this.toChucs.find(e => e.id === toChucId)?.displayName;
  }

  createUser(): void {
    this.showCreateOrEditUserDialog();
  }

  editUser(user: UserDto): void {
    this.showCreateOrEditUserDialog(user.id);
  }

  viewUser(user: UserDto): void {
    this.showCreateOrEditUserDialog(user.id, true);
  }

  updateRole(user: UserDto) {
    this.showCreateOrEditUserDialog(user.id, false, true);
  }

  public resetPassword(user: UserDto): void {
    this.showResetPasswordUserDialog(user.id);
  }

  protected delete(user: UserDto): void {
    if (user.lastLoginTime) {
      this.showExistMessage('Người dùng đã đăng nhập, không được xóa!');
    } else {
      const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
      '<p class="text-popup-xoa m-t-8">'
       + 'Người dùng ' + user.name + ' sẽ bị xóa.' + '</p>';
      this.swal.fire({
        html: html1,
        icon: 'warning',
        iconHtml: '<span class="icon1">&#9888</span>',
        showCancelButton: true,
        confirmButtonColor: this.confirmButtonColor,
        cancelButtonColor: this.cancelButtonColor,
        cancelButtonText: this.cancelButtonText,
        confirmButtonText: this.confirmButtonText
      }).then((result) => {
        if (result.value) {
          this._userService.delete(user.id).subscribe(() => {
            this.showDeleteMessage();
            this.getDataPage(false);
          });
        }
      });
    }
  }

  private showResetPasswordUserDialog(id?: number): void {
    this._modalService.show(ResetPasswordDialogComponent, {
      class: 'modal-xl',
      initialState: {
        id: id,
      },
    });
  }

  private showCreateOrEditUserDialog(id?: number, isView = false, isRoleActive = false): void {
    let createOrEditUserDialog: BsModalRef;
    if (!id) {
      createOrEditUserDialog = this._modalService.show(
        CreateUserDialogComponent,
        {
          class: 'modal-xl',
        }
      );
    } else {
      createOrEditUserDialog = this._modalService.show(
        EditUserDialogComponent,
        {
          class: 'modal-xl',
          initialState: {
            id: id,
            isView,
            isRoleActive
          },
        }
      );
    }

    createOrEditUserDialog.content.onSave.subscribe(() => {
      this.getDataPage(false);
    });
  }
}
