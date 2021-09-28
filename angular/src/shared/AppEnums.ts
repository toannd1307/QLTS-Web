// tslint:disable
import { TenantAvailabilityState } from '@shared/service-proxies/service-proxies';


export class AppTenantAvailabilityState {
    static Available: number = TenantAvailabilityState._1;
    static InActive: number = TenantAvailabilityState._2;
    static NotFound: number = TenantAvailabilityState._3;
}


export enum TrangThaiThucThiQueryConst {
    ThanhCong = 1,
    ThatBai,
}

export enum TrangThaiHieuLucConst {
    KhoiTao = 1,
    HieuLuc,
    HetHieuLuc
}

export enum TrangThaiDuyetConst {
    KhoiTao = 1,
    ChoDuyet,
    DaDuyet,
    KhongDuyet
}

export enum ReadExcelResultCodeConst {
    FileNotFound = 1,
    CantReadData,
    OK = 200
}


export enum TinhTrangMaSuDungTaiSanConst {
    RFID = 1,
    Barcode,
    QRCode
}

export enum TrangThaiTaiSanConst {
    CapPhat = 1,
    DieuChuyen,
    ThuHoi,
    TaiSanDangSuDung,
    TaiSanSuaChua,
    TaiSanBaoDuong,
    TaiSanMat,
    TaiSanHong,
    TaiSanThanhLy,
    TaiSanHuy,
}

export enum TrangThaiSuaChuaBaoDuongConst {
    DangThucHien = 1,
    ThanhCong,
    KhongThanhCong
}

export enum TrangThaiKiemKeConst {
    ChuaBatDau = 1,
    DangKiemKe,
    DaKetThuc
}

export enum TrangThaiTabConst {
    ToanBoTaiSan = 0,
    TaiSanChuaSuDung,
    TaiSanDangSuDung
}

