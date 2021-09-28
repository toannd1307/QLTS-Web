import { Component, Injector, Input, OnInit, ViewChild } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { CommonComponent } from '@shared/dft/components/common.component';
import { PermissionTreeEditModel } from '@shared/dft/components/permission-tree-edit.model';
import { BaoCaoThongTinTaiSanServiceProxy, ListBaoCaoChiTietDto, LookupTableServiceProxy } from '@shared/service-proxies/service-proxies';
import { Table } from 'primeng/table';
import { LazyLoadEvent } from 'primeng/api';
import { finalize } from 'rxjs/operators';
import * as moment from 'moment';
import { DashBoardConst } from './DashBoardConst';
import { FileDownloadService } from '@shared/file-download.service';

@Component({
  selector: 'app-bao-cao-thong-tin-tai-san',
  templateUrl: './bao-cao-thong-tin-tai-san.component.html',
  styleUrls: ['./bao-cao-thong-tin-tai-san.component.scss'],
  animations: [appModuleAnimation()]
})
export class BaoCaoThongTinTaiSanComponent extends AppComponentBase implements OnInit {

  @Input() isActive = 0;
  @ViewChild('dt') table: Table;
  tenPhongBan = '';
  loading = true;
  exporting = false;
  totalCount = 0;
  nowDate = moment(new Date()).format('DD/MM/YYYY');
  records: ListBaoCaoChiTietDto[] = [];
  input: ListBaoCaoChiTietDto[] = [];
  arrTaiSanChecked: ListBaoCaoChiTietDto[] = [];
  form: FormGroup;
  toChucValue: number[];
  toChucItems: PermissionTreeEditModel;
  selectedValues: string[] = [];
  rangeDates: any[] = [];
  data: any;
  barOptions1: any;

  constructor(
    injector: Injector,
    private _baoCaoThongTinTaiSanServiceProxy: BaoCaoThongTinTaiSanServiceProxy,
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

    this._baoCaoThongTinTaiSanServiceProxy.checkPhongBan().subscribe(rs => {
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
    this._baoCaoThongTinTaiSanServiceProxy.exportToExcel(
      this.rangeDates ? this.rangeDates[0] : undefined,
      this.rangeDates ? this.rangeDates[1] : undefined,
      undefined,
      this.toChucValue || undefined).subscribe((result) => {
        this._fileDownloadService.downloadTempFile(result);
        this.exporting = false;
      }, () => {
        this.exporting = false;
      });
  }

  getDataPage(isSearch: boolean, lazyLoad?: LazyLoadEvent) {
    this.arrTaiSanChecked = [];
    this.loading = true;
    this._baoCaoThongTinTaiSanServiceProxy.getAllBaoCao(
      this.toChucValue || undefined,
      this.rangeDates ? this.rangeDates[0] : undefined,
      this.rangeDates ? this.rangeDates[1] : undefined,
      isSearch,
    ).pipe(finalize(() => { this.loading = false; }))
      .subscribe(result => {
        this.records = result;
        this.totalCount = result.length;
        this.data = {
          labels: ['Đang sử dụng', 'Cấp phát', 'Thu hồi', 'Điều chuyển', 'Báo mất', 'Báo hỏng', 'Báo huỷ', 'Thanh lý', 'Dự trù mua sắm', 'Sửa chữa', 'Bảo dưỡng'],
          datasets: [
            {
              label: undefined,
              backgroundColor: [
                DashBoardConst.backGroundTealMTCL5,
                DashBoardConst.backGroundChiTieuMTCL,
                DashBoardConst.backGroundDatMTCL,
                DashBoardConst.backGroundDatCoGiaiTrinhMTCL,
                DashBoardConst.backGroundKhongDatMTCL,
                DashBoardConst.backGroundTealMTCL6,
                DashBoardConst.backGroundTealMTCL1,
                DashBoardConst.backGroundTealMTCL2,
                DashBoardConst.backGroundTealMTCL3,
                DashBoardConst.backGroundTealMTCL7,
                DashBoardConst.backGroundTealMTCL4],
              data:
                [
                  result[result.length - 1]?.listDangSuDung,
                  result[result.length - 1]?.listCapPhat,
                  result[result.length - 1]?.listThuHoi,
                  result[result.length - 1]?.listDieuChuyen,
                  result[result.length - 1]?.listBaoMat,
                  result[result.length - 1]?.listBaoHong,
                  result[result.length - 1]?.listBaoHuy,
                  result[result.length - 1]?.listThanhLy,
                  result[result.length - 1]?.listDuTruMuaSam,
                  result[result.length - 1]?.listSuaChua,
                  result[result.length - 1]?.listBaoDuong,
                ]
            }
          ]
        };
      });
  }
}

