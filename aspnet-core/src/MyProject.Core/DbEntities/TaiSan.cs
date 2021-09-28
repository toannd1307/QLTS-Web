﻿// This file is not generated, but this comment is necessary to exclude it from StyleCop analysis
// <auto-generated/>
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbEntities
{
    [Table("TaiSan")]
    public class TaiSan : FullAuditedEntity, IMayHaveTenant
    {
        public virtual int? TenantId { get; set; }
        public virtual DateTime? NgayMua { get; set; }
        public virtual string NgayMuaStr { get; set; }
        public virtual DateTime? NgayHetHanSuDung { get; set; }
        public virtual DateTime? NgayHetHanBaoHanh { get; set; }
        public virtual long? NguoiChotGiaId { get; set; }
        public virtual DateTime? ThoiDiemChotGia { get; set; }
        public virtual double? GiaCuoi { get; set; }
        public virtual string NoiDungChotGia { get; set; }
        public virtual string QRCode { get; set; }
        public virtual string BarCode { get; set; }
        public virtual string RFIDCode { get; set; }
        public virtual string EPCCode { get; set; }
        public virtual bool? TinhTrangSuDungQRCode { get; set; }
        public virtual bool? TinhTrangSuDungBarCode { get; set; }
        public virtual bool? TinhTrangSuDungRFIDCode { get; set; }
        public virtual int? ToChucId { get; set; }
        public virtual string GhiChu { get; set; }
        public virtual string TenTaiSan { get; set; }
        public virtual int? LoaiTaiSanId { get; set; }
        public virtual string SerialNumber { get; set; }
        public virtual string ProductNumber { get; set; }
        public virtual int? NhaCungCapId { get; set; }
        public virtual string HangSanXuat { get; set; }
        public virtual double? NguyenGia { get; set; }
        public virtual string NguyenGiaStr { get; set; }
        public virtual int? TrangThaiId { get; set; }
        public virtual int? ThoiGianTrichKhauHao { get; set; }
        public virtual string ReaderMacId { get; set; }
        public virtual int? NguonKinhPhiId { get; set; }
        public virtual ICollection<TaiSanDinhKemFile> TaiSanDinhKemFileList { get; set; }
    }

    [Table("vTaiSan")]
    public class ViewTaiSan : FullAuditedEntity
    {
        public virtual string TenTaiSan { get; set; }
        public virtual string SerialNumber { get; set; }
        public virtual string LoaiTaiSan { get; set; }
        public virtual int? LoaiTaiSanId { get; set; }
        public virtual string NhaCungCap { get; set; }
        public virtual double? NguyenGia { get; set; }
        public virtual string PhongBanQuanLy { get; set; }
        public virtual int? PhongBanQuanLyId { get; set; }
        public virtual int? HinhThuc { get; set; }
    }

    [Table("vTaiSanSuaChuaBaoDuong")]
    public class ViewTaiSanSuaChuaBaoDuong : FullAuditedEntity
    {
        public virtual string EPCCode { get; set; }
        public virtual string TenTaiSan { get; set; }
        public virtual string LoaiTaiSan { get; set; }
        public virtual int? LoaiTaiSanId { get; set; }
        public virtual string NhaCungCap { get; set; }
        public virtual double? NguyenGia { get; set; }
        public virtual string NguyenGiaStr { get; set; }
        public virtual string PhongBanQuanLy { get; set; }
        public virtual int? PhongBanQuanLyId { get; set; }
        public virtual int? HinhThuc { get; set; }
        public virtual string LiDoSuaChuaBaoDuong { get; set; }
        public virtual string DiaChiSuaChuaBaoDuong { get; set; }
        public virtual DateTime? ThoiGianBatDau { get; set; }
        public virtual int? TrangThai { get; set; }
        public virtual long? PhieuTaiSanChiTietId { get; set; }
    }

    [Table("vTaiSanMat")]
    public class ViewTaiSanMat : FullAuditedEntity
    {
        public virtual string EPCCode { get; set; }
        public virtual string TenTaiSan { get; set; }
        public virtual string LoaiTaiSan { get; set; }
        public virtual int? LoaiTaiSanId { get; set; }
        public virtual string SerialNumber { get; set; }
        public virtual string NhaCungCap { get; set; }
        public virtual DateTime? NgayMua { get; set; }
        public virtual string NgayMuaStr { get; set; }
        public virtual double? NguyenGia { get; set; }
        public virtual string NguyenGiaStr { get; set; }
        public virtual string PhongBanQuanLy { get; set; }
        public virtual int? PhongBanQuanLyId { get; set; }
        public virtual DateTime? NgayKhaiBao { get; set; }
        public virtual string NguyenNhan { get; set; }
    }

    [Table("vTaiSanHong")]
    public class ViewTaiSanHong : FullAuditedEntity
    {
        public virtual string EPCCode { get; set; }
        public virtual string TenTaiSan { get; set; }
        public virtual string LoaiTaiSan { get; set; }
        public virtual int? LoaiTaiSanId { get; set; }
        public virtual string SerialNumber { get; set; }
        public virtual string NhaCungCap { get; set; }
        public virtual DateTime? NgayMua { get; set; }
        public virtual string NgayMuaStr { get; set; }
        public virtual double? NguyenGia { get; set; }
        public virtual string NguyenGiaStr { get; set; }
        public virtual string PhongBanQuanLy { get; set; }
        public virtual int? PhongBanQuanLyId { get; set; }
        public virtual DateTime? NgayKhaiBao { get; set; }
        public virtual string NguyenNhan { get; set; }
    }

    [Table("vTaiSanThanhLy")]
    public class ViewTaiSanThanhLy : FullAuditedEntity
    {
        public virtual string EPCCode { get; set; }
        public virtual string TenTaiSan { get; set; }
        public virtual string LoaiTaiSan { get; set; }
        public virtual int? LoaiTaiSanId { get; set; }
        public virtual string SerialNumber { get; set; }
        public virtual string NhaCungCap { get; set; }
        public virtual DateTime? NgayMua { get; set; }
        public virtual string NgayMuaStr { get; set; }
        public virtual double? NguyenGia { get; set; }
        public virtual string NguyenGiaStr { get; set; }
        public virtual string PhongBanQuanLy { get; set; }
        public virtual int? PhongBanQuanLyId { get; set; }
        public virtual DateTime? NgayKhaiBao { get; set; }
        public virtual string NguyenNhan { get; set; }
    }

    [Table("vTaiSanHuy")]
    public class ViewTaiSanHuy : FullAuditedEntity
    {
        public virtual string EPCCode { get; set; }
        public virtual string TenTaiSan { get; set; }
        public virtual string LoaiTaiSan { get; set; }
        public virtual int? LoaiTaiSanId { get; set; }
        public virtual string SerialNumber { get; set; }
        public virtual string NhaCungCap { get; set; }
        public virtual DateTime? NgayMua { get; set; }
        public virtual string NgayMuaStr { get; set; }
        public virtual double? NguyenGia { get; set; }
        public virtual string NguyenGiaStr { get; set; }
        public virtual string PhongBanQuanLy { get; set; }
        public virtual int? PhongBanQuanLyId { get; set; }
        public virtual DateTime? NgayKhaiBao { get; set; }
        public virtual string NguyenNhan { get; set; }
    }
}