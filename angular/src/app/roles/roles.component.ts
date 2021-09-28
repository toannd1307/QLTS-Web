import { AppComponentBase } from '@shared/app-component-base';
// tslint:disable
import { finalize } from 'rxjs/operators';
import { Component, Injector, ViewChild } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  PagedListingComponentBase,
  PagedRequestDto
} from '@shared/paged-listing-component-base';
import {
  RoleServiceProxy,
  RoleDto,
  RoleDtoPagedResultDto,
  UserDtoPagedResultDto
} from '@shared/service-proxies/service-proxies';
import { CreateRoleDialogComponent } from './create-role/create-role-dialog.component';
import { EditRoleDialogComponent } from './edit-role/edit-role-dialog.component';
import { LazyLoadEvent } from 'primeng/api/public_api';
import { Table } from 'primeng/table';

class PagedRolesRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './roles.component.html',
  animations: [appModuleAnimation()]
})
export class RolesComponent extends AppComponentBase {
  @ViewChild('dt') table: Table;
  roles: RoleDto[] = [];
  keyword = '';
  loading = false;
  totalCount = 0;
  constructor(
    injector: Injector,
    private _rolesService: RoleServiceProxy,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  getDataPage(isSearch: boolean, lazyLoad?: LazyLoadEvent) {
    this._rolesService
      .getAll(
        this.keyword || undefined,
        isSearch,
        this.getSortField(this.table),
        lazyLoad ? lazyLoad.first : this.table.first,
        lazyLoad ? lazyLoad.rows : this.table.rows,
      ).subscribe((result: RoleDtoPagedResultDto) => {
        this.loading = false;
        this.roles = result.items;
        this.totalCount = result.totalCount;
      });
  }

  delete(role: RoleDto): void {
    const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
    '<p class="text-popup-xoa m-t-8">'
     + 'Vai trò ' + role.displayName + ' sẽ bị xóa.' + '</p>';
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
        this._rolesService
          .delete(role.id)
          .pipe(
            finalize(() => {
              this.showDeleteMessage();
              this.getDataPage(false);
            })
          )
          .subscribe(() => { });
      }
    });

  }

  createRole(): void {
    this.showCreateOrEditRoleDialog();
  }

  editRole(role: RoleDto, isPermissionActive = false): void {
    this.showCreateOrEditRoleDialog(role.id, isPermissionActive);
  }

  showCreateOrEditRoleDialog(id?: number, isPermissionActive = false): void {
    let createOrEditRoleDialog: BsModalRef;
    if (!id) {
      createOrEditRoleDialog = this._modalService.show(
        CreateRoleDialogComponent,
        {
          class: 'modal-xl',
        }
      );
    } else {
      createOrEditRoleDialog = this._modalService.show(
        EditRoleDialogComponent,
        {
          class: 'modal-xl',
          initialState: {
            id: id,
            isPermissionActive
          },
        }
      );
    }

    createOrEditRoleDialog.content.onSave.subscribe(() => {
      this.getDataPage(false);
    });
  }
}
