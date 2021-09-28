namespace MyProject.QuanLyTaiSan.Dtos
{
    using System;
    using Abp.Application.Services.Dto;
    using Abp.AutoMapper;
    using DbEntities;

    [AutoMap(typeof(PhieuTaiSan))]
    public class PhieuTaiSanDto : EntityDto<long>
    {
        public int? PhanLoaiId { get; set; }

        public DateTime? NgayKhaiBao { get; set; }

        public long? NguoiKhaiBaoId { get; set; }

        public int? ToChucKhaiBaoId { get; set; }

        public int? ToChucDuocNhanId { get; set; }

        public string NoiDung { get; set; }

        public string DiaChi { get; set; }

        public string GhiChu { get; set; }
    }
}
