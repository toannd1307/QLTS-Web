/* tslint:disable */
import { CommonComponent } from './../../../shared/dft/components/common.component';
import { StringLookupTableDto } from './../../../shared/service-proxies/service-proxies';
import { Component, ViewChild, Injector, OnInit } from "@angular/core";
import { appModuleAnimation } from "@shared/animations/routerTransition";
import { AppComponentBase } from "@shared/app-component-base";
import { FileDownloadService } from "@shared/file-download.service";
import { AuditLogServiceProxy, AuditLogListDto, GetAuditLogsInput, } from "@shared/service-proxies/service-proxies";
import { NotifyService } from "abp-ng2-module";
import { LazyLoadEvent } from "primeng/api";
import { Table } from "primeng/table";
import { uniq } from 'lodash';

@Component({
    templateUrl: './audit-logs.component.html',
    styleUrls: ['./audit-logs.component.less'],
    animations: [appModuleAnimation()]
})
export class AuditLogsComponent extends AppComponentBase implements OnInit {

    @ViewChild('dt') table: Table;
    public usernameAuditLog: string;
    public serviceName: StringLookupTableDto;
    totalCount = 0;
    primengTableHelperAuditLogs: AuditLogListDto[] = [];
    advancedFiltersAreShown = false;
    loading = true;
    exporting = false;
    keyword = '';
    input: GetAuditLogsInput;
    arrService: StringLookupTableDto[] = [];
    rangeDates: any[] = [];

    constructor(
        injector: Injector,
        private _auditLogService: AuditLogServiceProxy,
        private _fileDownloadService: FileDownloadService
    ) {
        super(injector);
    }
    ngOnInit(): void {
        this.rangeDates[0] = CommonComponent.getNgayDauTienCuaThangHienTaiDate();
        this.rangeDates[1] = new Date();

        this._auditLogService.getAllServiceName().subscribe(res => {
            this.arrService = res;
        })
    }

    getAuditLogs(isSearch:boolean, lazyLoad?: LazyLoadEvent) {
        this.loading = true;
        this._auditLogService.getAllAuditLogs(
            this.rangeDates ? this.rangeDates[0] : undefined,
            this.rangeDates ? this.rangeDates[1] : undefined,
            this.usernameAuditLog || undefined,
            this.serviceName ? this.serviceName.id : undefined,
            isSearch,
            this.getSortField(this.table),
            lazyLoad ? lazyLoad.first : this.table.first,
            lazyLoad ? lazyLoad.rows : this.table.rows,
        ).subscribe((result) => {
            this.loading = false;
            this.totalCount = result.totalCount;
            this.primengTableHelperAuditLogs = result.items;
        });
    }

    exportToExcelAuditLogs(): void {

        const self = this;
        self.exporting = true;
        this.input = new GetAuditLogsInput();
        this.input.startDate = this.rangeDates ? this.rangeDates[0] : undefined;
        this.input.endDate = this.rangeDates ? this.rangeDates[1] : undefined;
        this.input.userName = this.usernameAuditLog || undefined;
        this.input.serviceName = this.serviceName ? this.serviceName.id : undefined;
        this.input.sorting = this.getSortField(this.table);
        this.input.skipCount = 0;
        this.input.maxResultCount = 10000000;
        this.input.startDate = this.rangeDates ? this.rangeDates[0] : undefined;
        this.input.endDate = this.rangeDates ? this.rangeDates[1] : undefined;
        self._auditLogService.exportToExcel(this.input)
            .subscribe(result => {
                self._fileDownloadService.downloadTempFile(result);
                self.exporting = false;
            });
    }

    truncateStringWithPostfix(text: string, length: number): string {
        return abp.utils.truncateStringWithPostfix(text, length);
    }
}
