import { Component, Injector, Input, OnInit, ViewChild } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { CommonComponent } from '@shared/dft/components/common.component';
import { PermissionTreeEditModel } from '@shared/dft/components/permission-tree-edit.model';
import { FileDownloadService } from '@shared/file-download.service';
import { BaoCaoCanhBaoServiceProxy, ListBaoCaoCanhBaoOutputDto, LookupTableServiceProxy } from '@shared/service-proxies/service-proxies';
import * as moment from 'moment';
import { Table } from 'primeng/table';
import { finalize } from 'rxjs/operators';
import { DashBoardConst } from '../bao-cao-thong-tin-tai-san/DashBoardConst';

@Component({
  selector: 'app-bao-cao-canh-bao',
  templateUrl: './bao-cao-canh-bao.component.html',
  styleUrls: ['./bao-cao-canh-bao.component.scss'],
  animations: [appModuleAnimation()]
})
export class BaoCaoCanhBaoComponent extends AppComponentBase implements OnInit {

  @Input() isActive = 0;
  @ViewChild('dt') table: Table;
  tenPhongBan = '';
  loading = true;
  exporting = false;
  totalCount = 0;
  nowDate = moment(new Date()).format('DD/MM/YYYY');
  records: ListBaoCaoCanhBaoOutputDto[] = [];
  input: ListBaoCaoCanhBaoOutputDto[] = [];
  arrTaiSanChecked: ListBaoCaoCanhBaoOutputDto[] = [];
  form: FormGroup;
  toChucValue: number[];
  toChucItems: PermissionTreeEditModel;
  selectedValues: string[] = [];
  rangeDates: any[] = [];
  data: any;
  barOptions1: any;

  constructor(
    injector: Injector,
    private _baoCaoCanhBaoServiceProxy: BaoCaoCanhBaoServiceProxy,
    private _lookupTableService: LookupTableServiceProxy,
    private _fileDownloadService: FileDownloadService,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.rangeDates[0] = CommonComponent.getNgayDauTienCuaThangHienTaiDatessss();
    this.rangeDates[1] = new Date();

    this._lookupTableService.getAllToChucTheoNguoiDung().subscribe((data) => {
      this.toChucItems = {
        data,
        selectedData: data.map(e => e.id),
      };
      this.toChucValue = data.map(e => e.id);
      this.getDataPage(false);
    });

    this._baoCaoCanhBaoServiceProxy.checkPhongBan().subscribe(rs => {
      this.tenPhongBan = rs;
    });

    this.barOptions1 = {
      legend: {
        position: 'bottom',
        display: false,
        onClick: null,
      },

      hover: {
        animationDuration: 0,
      },
      animation: DashBoardConst.animationBar,
      scales: {
        yAxes: [{
          display: true,
          ticks: {
            beginAtZero: true,
          },
        }],
      },
    };

  }

  exportToExcel() {
    this.exporting = true;
    this._baoCaoCanhBaoServiceProxy.exportToExcel(
      this.rangeDates ? this.rangeDates[0] : undefined,
      this.rangeDates ? this.rangeDates[1] : undefined,
      this.toChucValue || undefined).subscribe((result) => {
        this._fileDownloadService.downloadTempFile(result);
        this.exporting = false;
      }, () => {
        this.exporting = false;
      });
  }

  getDataPage(isSearch: boolean) {
    this.arrTaiSanChecked = [];
    this.loading = true;
    this._baoCaoCanhBaoServiceProxy.getAllBaoCao(
      this.toChucValue || undefined,
      this.rangeDates ? this.rangeDates[0] : undefined,
      this.rangeDates ? this.rangeDates[1] : undefined,
      isSearch,
    ).pipe(finalize(() => { this.loading = false; }))
      .subscribe(result => {
        this.records = result;
        this.totalCount = result.length;
        this.data = {
          labels: ['Tài sản ra', 'Tài sản vào', 'Bắt đầu kiểm kê', 'Kết thúc kiểm kê'],
          datasets: [
            {
              label: undefined,
              backgroundColor: [
                DashBoardConst.backGroundTealMTCL5,
                DashBoardConst.backGroundChiTieuMTCL,
                DashBoardConst.backGroundDatMTCL,
                DashBoardConst.backGroundDatCoGiaiTrinhMTCL,
              ],
              data:
                [
                  result[result.length - 1]?.taiSanRa,
                  result[result.length - 1]?.taiSanVao,
                  result[result.length - 1]?.batDauKiemKe,
                  result[result.length - 1]?.ketThucKiemKe,
                ]
            }
          ]
        };
      });
  }
}


