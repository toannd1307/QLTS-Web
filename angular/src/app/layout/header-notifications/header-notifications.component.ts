import { ChangeDetectionStrategy, Component, Injector, OnInit, ViewChild } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { CanhBaoServiceProxy, ThongBaoOutput } from '@shared/service-proxies/service-proxies';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';

@Component({
  selector: 'app-header-notifications',
  templateUrl: './header-notifications.component.html',
  styleUrls: ['./header-notifications.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HeaderNotificationsComponent extends AppComponentBase implements OnInit {

  @ViewChild('dt') table: Table;

  readStateFilter = 'ALL';
  loading = false;
  records: ThongBaoOutput[] = [];
  totalCount = 0;
  toChuc: string;
  exporting = false;

  constructor(
    injector: Injector,
    private _notificationService: CanhBaoServiceProxy,
  ) {
    super(injector);
  }
  ngOnInit(): void {
    this.getDataPage();
  }

  getDataPage(): void {
    this._notificationService.getAllThongBao(
      this.getSortField(this.table),
      0,
      5,
    ).subscribe((result) => {
      this.totalCount = result.totalCount;
      this.records = result.items;
    });
  }
}
