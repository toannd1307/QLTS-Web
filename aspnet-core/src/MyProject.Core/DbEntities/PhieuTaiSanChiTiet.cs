// This file is not generated, but this comment is necessary to exclude it from StyleCop analysis
// <auto-generated/>
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbEntities
{
    [Table("PhieuTaiSanChiTiet")]
    public class PhieuTaiSanChiTiet : FullAuditedEntity<long>, IMayHaveTenant
    {
        public virtual int? TenantId { get; set; }
        public virtual long? PhieuTaiSanId { get; set; }
        public virtual int? TaiSanId { get; set; }
        public virtual int? TrangThaiId { get; set; }
        public virtual int? ViTriLapDat { get; set; }
        public virtual int? ToChucDangQuanLyId { get; set; }
        public virtual string GhiChu { get; set; }
    }

    [Table("vPhieuTaiSanChiTiet")]
    public class ViewPhieuTaiSanChiTiet : FullAuditedEntity<long>
    {
        public virtual long? PhieuTaiSanId { get; set; }
        public virtual int? TaiSanId { get; set; }
        public virtual int? TrangThaiId { get; set; }
        public virtual int? ViTriLapDat { get; set; }
        public virtual string GhiChu { get; set; }
        public virtual string TenTaiSan { get; set; }
        public virtual int LoaiTaiSanId { get; set; }
        public virtual string LoaiTaiSan { get; set; }
        public virtual string SerialNumber { get; set; }
        public virtual int NhaCungCapId { get; set; }
        public virtual string NhaCungCap { get; set; }
        public virtual DateTime NgayMua { get; set; }
        public virtual double NguyenGia { get; set; }
        public virtual int PhongBanQuanLyId { get; set; }
        public virtual string PhongBanQuanLy { get; set; }
    }
}
