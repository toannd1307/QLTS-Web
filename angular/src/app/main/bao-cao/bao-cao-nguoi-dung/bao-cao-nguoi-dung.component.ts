import { Component, Injector, Input, OnInit, ViewChild } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { CommonComponent } from '@shared/dft/components/common.component';
import { PermissionTreeEditModel } from '@shared/dft/components/permission-tree-edit.model';
import { FileDownloadService } from '@shared/file-download.service';
import { BaoCaoNguoiDungServiceProxy, ListViewDto, LookupTableServiceProxy } from '@shared/service-proxies/service-proxies';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';
import { finalize } from 'rxjs/operators';
import { DashBoardConst } from '../bao-cao-thong-tin-tai-san/DashBoardConst';


@Component({
  selector: 'app-bao-cao-nguoi-dung',
  templateUrl: './bao-cao-nguoi-dung.component.html',
  styleUrls: ['./bao-cao-nguoi-dung.component.scss'],
  animations: [appModuleAnimation()]
})
export class BaoCaoNguoiDungComponent extends AppComponentBase implements OnInit {

  @Input() isActive = 0;
  @ViewChild('dt') table: Table;
  tenPhongBan = '';
  loading = true;
  exporting = false;
  totalCount = 0;
  records: ListViewDto[] = [];
  arrTaiSanChecked: ListViewDto[] = [];
  toChucValue: number[];
  toChucItems: PermissionTreeEditModel;
  selectedValues: string[] = [];
  rangeDates: any[] = [];
  data: any;
  barOptions1: any;

  constructor(
    injector: Injector,
    private _baoCaoNguoiDungServiceProxy: BaoCaoNguoiDungServiceProxy,
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
    this._baoCaoNguoiDungServiceProxy.exportToExcel(
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
    this._baoCaoNguoiDungServiceProxy.getAllBaoCao(
      this.toChucValue || undefined,
      this.rangeDates ? this.rangeDates[0] : undefined,
      this.rangeDates ? this.rangeDates[1] : undefined,
      isSearch,
    ).pipe(finalize(() => { this.loading = false; }))
      .subscribe(result => {
        this.records = result;
        this.totalCount = result.length;
        this.data = {
          labels: ['Đăng nhập', 'Đăng xuất', 'Thêm mới', 'Sửa', 'Xoá', 'Xem', 'Xuất Excel', 'Nhập Excel', 'Hoàn tác', 'Khai báo sử dụng', 'Cấp phát', 'Điều chuyển', 'Thu hồi', 'Báo hỏng mất', 'Tìm kiếm'],
          datasets: [
            {
              label: undefined,
              backgroundColor: [
                DashBoardConst.backGroundChiTieuMTCL,
                DashBoardConst.backGroundDatMTCL,
                DashBoardConst.backGroundDatCoGiaiTrinhMTCL,
                DashBoardConst.backGroundKhongDatMTCL,
                DashBoardConst.backGroundTealMTCL6,
                DashBoardConst.backGroundTealMTCL1,
                DashBoardConst.backGroundTealMTCL10,
                DashBoardConst.backGroundTealMTCL5,
                DashBoardConst.backGroundTealMTCL2,
                DashBoardConst.backGroundTealMTCL3,
                DashBoardConst.backGroundTealMTCL7,
                DashBoardConst.backGroundTealMTCL4,
                DashBoardConst.backGroundTealMTCL8,
                DashBoardConst.backGroundTealMTCL9,
                DashBoardConst.backGroundChiTieu],
              data:
                [
                  result[result.length - 1]?.listLogin,
                  result[result.length - 1]?.listLogout,
                  result[result.length - 1]?.listCreate,
                  result[result.length - 1]?.listEdit,
                  result[result.length - 1]?.listDelete,
                  result[result.length - 1]?.listView,
                  result[result.length - 1]?.listDownLoad,
                  result[result.length - 1]?.listUpload,
                  result[result.length - 1]?.listHoanTac,
                  result[result.length - 1]?.listKhaiBaoSuDung,
                  result[result.length - 1]?.listCapPhat,
                  result[result.length - 1]?.listDieuChuyen,
                  result[result.length - 1]?.listThuHoi,
                  result[result.length - 1]?.listHongMatTaiSan,
                  result[result.length - 1]?.listSearch,
                ]
            }
          ]
        };
      });
  }
}
