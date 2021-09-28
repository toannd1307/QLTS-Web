import { Component, EventEmitter, Injector, OnInit, Output, ViewChild, AfterViewInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AppComponentBase } from '@shared/app-component-base';
import { LookupTableDto, KhaiBaoHongMatServiceProxy } from '@shared/service-proxies/service-proxies';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-xem-chi-tiet-tai-san-hong-mat',
  templateUrl: './xem-chi-tiet-tai-san-hong-mat.component.html',
  styleUrls: ['./xem-chi-tiet-tai-san-hong-mat.component.scss']
})
export class XemChiTietTaiSanHongMatComponent extends AppComponentBase implements OnInit, AfterViewInit {

  @Output() onSave = new EventEmitter<any>();
  form: FormGroup;
  saving = false;
  isEdit = false;
  id: number;
  isView = false;
  @ViewChild('dt') table: Table;
  loading = true;
  exporting = false;
  keyword = '';
  totalCount = 0;
  records: any[] = [];
  suggestionsSingle: LookupTableDto[];
  formSearch: FormGroup;

  constructor(
    injector: Injector,
    private _khaiBaoHongMatServiceProxy: KhaiBaoHongMatServiceProxy,
    private _fb: FormBuilder,
    public bsModalRef: BsModalRef,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.khoiTaoForm();
  }

  ngAfterViewInit() {
    this.getDataPage();
  }

  khoiTaoForm() {
    this.form = this._fb.group({
      nguoiKhaiBao: [''],
      ngayKhaiBao: [''],
      noiDungKhaiBao: [''],
    });
  }

  close() {
    this.bsModalRef.hide();
  }

  getDataPage(lazyLoad?: LazyLoadEvent) {
    this.loading = true;
    this._khaiBaoHongMatServiceProxy.getAllKhaiBaoHongMatChiTiet(
      this.id,
      this.getSortField(this.table),
      lazyLoad ? lazyLoad.first : this.table.first,
      lazyLoad ? lazyLoad.rows : this.table.rows,
    ).pipe(
      finalize(() => { this.loading = false; })
    ).subscribe(result => {

      this.form.get('nguoiKhaiBao').setValue(result.items[0].nguoiKhaiBao);
      this.form.get('ngayKhaiBao').setValue(result.items[0].ngayKhaiBao.toDate());
      this.form.get('noiDungKhaiBao').setValue(result.items[0].noiDungKhaiBao);

      this.records = result.items;
      this.totalCount = result.totalCount;
    });
  }
}
