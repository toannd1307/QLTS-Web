import { Component, Injector, OnInit, ViewChild, OnDestroy, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { LookupTableDto, LookupTableServiceProxy, ViewKhaiBaoHongMat, KhaiBaoHongMatServiceProxy } from '@shared/service-proxies/service-proxies';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { TreeviewItem } from '@shared/dft/dropdown-treeview-select/lib/models/treeview-item';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';
import { Subscription, forkJoin } from 'rxjs';
import { finalize, map } from 'rxjs/operators';
import { XemChiTietTaiSanHongMatComponent } from './xem-chi-tiet-tai-san-hong-mat/xem-chi-tiet-tai-san-hong-mat.component';
import { PermissionTreeEditModel } from '@shared/dft/components/permission-tree-edit.model';

@Component({
  selector: 'app-khai-bao-hong-mat-tai-san',
  templateUrl: './khai-bao-hong-mat-tai-san.component.html',
  styleUrls: ['./khai-bao-hong-mat-tai-san.component.scss'],
  animations: [appModuleAnimation()],
})
export class KhaiBaoHongMatTaiSanComponent extends AppComponentBase implements OnInit, OnDestroy, OnChanges {

  @Input() isActive = 0;
  @ViewChild('dt') table: Table;
  loading = true;
  exporting = false;
  totalCount = 0;
  records: ViewKhaiBaoHongMat[] = [];
  // input: TaiSanGetAllInputDto;
  suggestionsSingle: LookupTableDto[];
  form: FormGroup;

  toChucValue: number[];

  toChucItems: PermissionTreeEditModel;
  arrNhaCungCap: LookupTableDto[];
  arrHinhThuc: LookupTableDto[];
  arrTrangThai: LookupTableDto[];

  arrTaiSanChecked: ViewKhaiBaoHongMat[] = [];
  selectedValues: string[] = [];

  arrKhaiBao: any[] = [];

  tenTaiSan = '';

  private _unsubscribe: Subscription[] = [];

  constructor(injector: Injector,
    private _modalService: BsModalService,
    private _taiSanServiceProxy: KhaiBaoHongMatServiceProxy,
    private _lookupTableService: LookupTableServiceProxy,
    private _fb: FormBuilder,
  ) {
    super(injector);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.isActive?.currentValue === 8) {
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
      this._lookupTableService.getAllTrangThaiTaiSan(),
    ).subscribe(([khaiBao]) => {
      this.arrKhaiBao = khaiBao.filter(ff =>
        ff.id === this.enums.TrangThaiTaiSanConst.TaiSanHong ||
        ff.id === this.enums.TrangThaiTaiSanConst.TaiSanMat);
    });
  }

  ngOnDestroy(): void {
    this._unsubscribe.forEach((sb) => sb.unsubscribe());
  }

  getDataPage(isSearch: boolean, lazyLoad?: LazyLoadEvent) {
    this.arrTaiSanChecked = [];
    this.loading = true;
    this._taiSanServiceProxy.getAllKhaiBaoHongMat(
      this.form?.value.timKiemKhaiBao || undefined,
      this.toChucValue || undefined,
      this.form?.value.khaiBao.id || undefined,
      isSearch,
      this.getSortField(this.table),
      lazyLoad ? lazyLoad.first : this.table?.first,
      lazyLoad ? lazyLoad.rows : this.table?.rows,
    ).pipe(finalize(() => { this.loading = false; }))
      .subscribe(result => {
        this.records = result.items;
        this.totalCount = result.totalCount;
      });
  }

  watch(id?: number) {
    this._showCreateOrEditDemoDialog(id);
  }


  private _showCreateOrEditDemoDialog(id?: number, isView = false): void {
    // copy
    let createOrEditUserDialog: BsModalRef;
    createOrEditUserDialog = this._modalService.show(
      XemChiTietTaiSanHongMatComponent,
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
    if (item === 'khaiBao') {
      this._lookupTableService.getAllTrangThaiTaiSan().pipe(
        map(f => f.filter(ff =>
          ff.id === this.enums.TrangThaiTaiSanConst.TaiSanHong ||
          ff.id === this.enums.TrangThaiTaiSanConst.TaiSanMat)),
      ).subscribe(result => {
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
      timKiemKhaiBao: [''],
      phongBanKhaiBao: [''],
      khaiBao: [''],
    });
  }
}
