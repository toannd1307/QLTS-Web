import { Component, Injector, OnInit, ViewChild, OnDestroy, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { ViewTaiSanSuaChuaBaoDuong, LookupTableDto, LookupTableServiceProxy, ViewTaiSanThanhLy, TaiSanThanhLyServiceProxy } from '@shared/service-proxies/service-proxies';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';
import { Subscription, forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { CreateOrEditTaiSanComponent } from '../toan-bo-tai-san/create-or-edit-tai-san/create-or-edit-tai-san.component';
import { CreateOrEditTaiSanThanhLyComponent } from './create-or-edit-tai-san-thanh-ly/create-or-edit-tai-san-thanh-ly.component';
import { PermissionTreeEditModel } from '@shared/dft/components/permission-tree-edit.model';

@Component({
  selector: 'app-tai-san-thanh-ly',
  templateUrl: './tai-san-thanh-ly.component.html',
  styleUrls: ['./tai-san-thanh-ly.component.scss'],
  animations: [appModuleAnimation()],
})
export class TaiSanThanhLyComponent extends AppComponentBase implements OnInit, OnDestroy, OnChanges {

  constructor(injector: Injector,
    private _modalService: BsModalService,
    private _taiSanServiceProxy: TaiSanThanhLyServiceProxy,
    private _lookupTableService: LookupTableServiceProxy,
    private _fb: FormBuilder,
  ) {
    super(injector);
  }

  @Input() isActive = 0;
  @ViewChild('dt') table: Table;
  loading = true;
  exporting = false;
  totalCount = 0;
  records: ViewTaiSanThanhLy[] = [];
  // input: TaiSanGetAllInputDto;
  suggestionsSingle: LookupTableDto[];
  form: FormGroup;

  toChucValue: number[];

  toChucItems: PermissionTreeEditModel;
  loaiTaiSanItems: TreeviewItem[];
  arrNhaCungCap: LookupTableDto[];

  arrTaiSanChecked: ViewTaiSanThanhLy[] = [];
  selectedValues: string[] = [];

  tenTaiSan = '';

  private _unsubscribe: Subscription[] = [];

  checkTaiSan24h = this.showUndoButton;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.isActive?.currentValue === 6) {
      this.getDataPage(false);
      this.arrTaiSanChecked = [];
    }
  }
  ngOnInit(): void {

    this.formCreate();
    this.bindingData();
    this._lookupTableService.getAllToChucTheoNguoiDung().subscribe((data) => {
      this.toChucItems = {
        data,
        selectedData: data.map(e => e.id),
      };
      this.toChucValue = data.map(e => e.id);
      this.getDataPage(false);
    });
  }

  bindingData() {
    forkJoin(
      this._lookupTableService.getAllLoaiTaiSanTree(),
      this._lookupTableService.getAllNhaCungCap(),
    ).subscribe(([loaiTaiSan, nhaCungCap]) => {
      this.loaiTaiSanItems = this.getTreeviewItem(loaiTaiSan);
      this.arrNhaCungCap = nhaCungCap;
    });
  }

  ngOnDestroy(): void {
    this._unsubscribe.forEach((sb) => sb.unsubscribe());
  }

  getDataPage(isSearch: boolean, lazyLoad?: LazyLoadEvent) {
    this.arrTaiSanChecked = [];
    this.loading = true;
    this._taiSanServiceProxy.getAllTaiSanThanhLy(
      this.form.value.tenTaiSan || undefined,
      this.toChucValue || undefined,
      this.form.value.loaiTaiSan || undefined,
      this.form.value.nhaCungCap?.displayName || undefined,
      isSearch,
      this.getSortField(this.table),
      lazyLoad ? lazyLoad.first : this.table.first,
      lazyLoad ? lazyLoad.rows : this.table.rows,
    ).pipe(finalize(() => { this.loading = false; }))
      .subscribe(result => {
        this.records = result.items;
        this.totalCount = result.totalCount;
      });
  }

  create(id?: number) {
    this._showCreateOrEditDemoDialog(id);
  }

  delete(record: ViewTaiSanSuaChuaBaoDuong) {
    this._lookupTableService.getTrangThaiTaiSanTruoc(record.id).subscribe(trangThaiTaiSanName => {
      const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
      '<p class="text-popup-xoa m-t-8">'
       + 'Tài sản ' + record.tenTaiSan + ' sẽ hoàn tác về "Danh sách Tài sản ' + trangThaiTaiSanName + '"' + '</p>';
      this.swal.fire({
        html: html1,
        icon: 'warning',
        iconHtml: '<span class="icon1">&#9888</span>',
        showCancelButton: true,
        confirmButtonColor: this.confirmButtonColor,
        cancelButtonColor: this.cancelButtonColor,
        cancelButtonText: this.cancelButtonText,
        confirmButtonText: 'Hoàn tác'
      }).then((result) => {
        if (result.value) {
          this.clearArrChecked([record]);
        }
      });
    });
  }

  deleteArrTaiSanChecked() {
    this.tenTaiSan = this.arrTaiSanChecked.filter(f => this.checkTaiSan24h(f.lastModificationTime)).map(m => { return m.tenTaiSan; }).join(', ');
    const html1 =  '<h3 class="title-popup-xoa m-t-24" >' + 'Bạn có chắc chắn không?' + '</h3>' +
    '<p class="text-popup-xoa m-t-8">'
     + 'Danh sách tài sản đã chọn sẽ được hoàn tác về danh sách trước khi khai báo thanh lý' + '</p>';
    this.swal.fire({
      html: html1,
      icon: 'warning',
      iconHtml: '<span class="icon1">&#9888</span>',
      showCancelButton: true,
      confirmButtonColor: this.confirmButtonColor,
      cancelButtonColor: this.cancelButtonColor,
      cancelButtonText: this.cancelButtonText,
      confirmButtonText: 'Hoàn tác'
    }).then((result) => {
      if (result.value) {
        this.clearArrChecked(this.arrTaiSanChecked.filter(f => this.checkTaiSan24h(f.lastModificationTime)));
      }
    });
  }

  clearArrChecked(arrTaiSanChecked) {
    this._taiSanServiceProxy.hoanTacTaiSanThanhLy(arrTaiSanChecked.map(m => { return { id: m.id }; })).subscribe(() => {
      this.arrTaiSanChecked = [];
      this.showSuccessMessage('Hoàn tác thành công!');
      this.getDataPage(false);
    });
  }

  private _showCreateOrEditDemoDialog(id?: number, isView = false): void {
    // copy
    let createOrEditUserDialog: BsModalRef;
    createOrEditUserDialog = this._modalService.show(
      CreateOrEditTaiSanThanhLyComponent,
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
    createOrEditUserDialog.content.onSave.subscribe(() => {
      this.getDataPage(false);
    });
  }

  view(id?: number) {
    this._showCreateOrEditTaiSanDialog(id, true);
  }

  private _showCreateOrEditTaiSanDialog(id?: number, isView = false): void {
    // copy
    let createOrEditUserDialog: BsModalRef;
    createOrEditUserDialog = this._modalService.show(
      CreateOrEditTaiSanComponent,
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
    createOrEditUserDialog.content.onSave.subscribe(() => {
      this.getDataPage(false);
    });
  }

  search(event, item) {
    const query = event.query;
    if (item === 'nhaCungCap') {
      this._lookupTableService.getAllNhaCungCap().subscribe(result => {
        this.suggestionsSingle = this.filter(query, result);
      });
    }
  }

  filter(query, items: LookupTableDto[]): any[] {
    const filtered: any[] = [];
    for (const iterator of items) {
      if (iterator.displayName.toLowerCase().indexOf(query.toLowerCase()) === 0) {
        filtered.push(iterator);
      }
    }
    return filtered;
  }

  formCreate() {
    this.form = this._fb.group({
      tenTaiSan: [''],
      phongBanQuanLy: [''],
      loaiTaiSan: [''],
      nhaCungCap: [],
    });
  }
}

