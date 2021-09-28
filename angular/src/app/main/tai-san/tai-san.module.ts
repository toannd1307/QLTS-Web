import { ThemKhaiBaoSuDungTaiSanComponent } from './dung-chung/khai-bao-su-dung-tai-san/them-khai-bao-su-dung-tai-san/them-khai-bao-su-dung-tai-san.component';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { CapPhatTaiSanComponent } from './dung-chung/cap-phat-tai-san/cap-phat-tai-san.component';
import { ThemCapPhatTaiSanComponent } from './dung-chung/cap-phat-tai-san/them-cap-phat-tai-san/them-cap-phat-tai-san.component';
import { DieuChuyenTaiSanComponent } from './dung-chung/dieu-chuyen-tai-san/dieu-chuyen-tai-san.component';
import { KhaiBaoSuDungTaiSanComponent } from './dung-chung/khai-bao-su-dung-tai-san/khai-bao-su-dung-tai-san.component';
import {
  ThemDieuChuyenTaiSanComponent
} from './dung-chung/dieu-chuyen-tai-san/them-dieu-chuyen-tai-san/them-dieu-chuyen-tai-san.component';
import { ThemThuHoiTaiSanComponent } from './dung-chung/thu-hoi-tai-san/them-thu-hoi-tai-san/them-thu-hoi-tai-san.component';
import { ThuHoiTaiSanComponent } from './dung-chung/thu-hoi-tai-san/thu-hoi-tai-san.component';
import { KhaiBaoHongMatTaiSanComponent } from './khai-bao-hong-mat-tai-san/khai-bao-hong-mat-tai-san.component';
import { CreateOrEditTaiSanChuaSuDungComponent } from './tai-san-chua-su-dung/create-or-edit-tai-san-chua-su-dung/create-or-edit-tai-san-chua-su-dung.component';
import { TaiSanChuaSuDungComponent } from './tai-san-chua-su-dung/tai-san-chua-su-dung.component';
import { TaiSanDangSuDungComponent } from './tai-san-dang-su-dung/tai-san-dang-su-dung.component';
import { CreateOrEditTaiSanHongComponent } from './tai-san-hong/create-or-edit-tai-san-hong/create-or-edit-tai-san-hong.component';
import { TaiSanHongComponent } from './tai-san-hong/tai-san-hong.component';
import { CreateOrEditTaiSanHuyComponent } from './tai-san-huy/create-or-edit-tai-san-huy/create-or-edit-tai-san-huy.component';
import { TaiSanHuyComponent } from './tai-san-huy/tai-san-huy.component';
import { CreateOrEditTaiSanMatComponent } from './tai-san-mat/create-or-edit-tai-san-mat/create-or-edit-tai-san-mat.component';
import { TaiSanMatComponent } from './tai-san-mat/tai-san-mat.component';
import { TaiSanRoutingModule } from './tai-san-routing.module';
import { CreateOrEditTaiSanSuaChuaBaoDuongComponent } from './tai-san-sua-chua-bao-duong/create-or-edit-tai-san-sua-chua-bao-duong/create-or-edit-tai-san-sua-chua-bao-duong.component';
import { TaiSanSuaChuaBaoDuongComponent } from './tai-san-sua-chua-bao-duong/tai-san-sua-chua-bao-duong.component';
import { CreateOrEditTaiSanThanhLyComponent } from './tai-san-thanh-ly/create-or-edit-tai-san-thanh-ly/create-or-edit-tai-san-thanh-ly.component';
import { TaiSanThanhLyComponent } from './tai-san-thanh-ly/tai-san-thanh-ly.component';
import { TaiSanComponent } from './tai-san.component';
import { CreateOrEditTaiSanComponent } from './toan-bo-tai-san/create-or-edit-tai-san/create-or-edit-tai-san.component';
import { ToanBoTaiSanComponent } from './toan-bo-tai-san/toan-bo-tai-san.component';
import { ThemTaiSanSuaChuaBaoDuongComponent } from './tai-san-sua-chua-bao-duong/them-tai-san-sua-chua-bao-duong/them-tai-san-sua-chua-bao-duong.component';
import { ThemTaiSanMatComponent } from './tai-san-mat/them-tai-san-mat/them-tai-san-mat.component';
import { ThemTaiSanHongComponent } from './tai-san-hong/them-tai-san-hong/them-tai-san-hong.component';
import { ThemTaiSanThanhLyComponent } from './tai-san-thanh-ly/them-tai-san-thanh-ly/them-tai-san-thanh-ly.component';
import { ThemTaiSanHuyComponent } from './tai-san-huy/them-tai-san-huy/them-tai-san-huy.component';
import { XemChiTietTaiSanHongMatComponent } from './khai-bao-hong-mat-tai-san/xem-chi-tiet-tai-san-hong-mat/xem-chi-tiet-tai-san-hong-mat.component';
import { SharedModule } from '@shared/shared.module';
import { AppModule } from '@app/app.module';
@NgModule({
  declarations: [
    TaiSanComponent,
    KhaiBaoHongMatTaiSanComponent,
    TaiSanChuaSuDungComponent,
    CreateOrEditTaiSanChuaSuDungComponent,
    CapPhatTaiSanComponent,
    DieuChuyenTaiSanComponent,
    ThemCapPhatTaiSanComponent,
    ThemDieuChuyenTaiSanComponent,
    ThuHoiTaiSanComponent,
    ThemThuHoiTaiSanComponent,
    ThemKhaiBaoSuDungTaiSanComponent,
    KhaiBaoSuDungTaiSanComponent,
    TaiSanDangSuDungComponent,
    TaiSanHongComponent,
    CreateOrEditTaiSanHongComponent,
    TaiSanHuyComponent,
    CreateOrEditTaiSanHuyComponent,
    TaiSanMatComponent,
    CreateOrEditTaiSanMatComponent,
    TaiSanSuaChuaBaoDuongComponent,
    CreateOrEditTaiSanSuaChuaBaoDuongComponent, TaiSanThanhLyComponent,
    CreateOrEditTaiSanThanhLyComponent,
    ToanBoTaiSanComponent,
    CreateOrEditTaiSanComponent,
    ThemTaiSanSuaChuaBaoDuongComponent,
    ThemTaiSanMatComponent,
    ThemTaiSanHongComponent,
    ThemTaiSanThanhLyComponent,
    ThemTaiSanHuyComponent,
    XemChiTietTaiSanHongMatComponent],
  imports: [
    CommonModule,
    SharedModule,
    TaiSanRoutingModule,
    AppModule,
  ]
})
export class TaiSanModule { }
