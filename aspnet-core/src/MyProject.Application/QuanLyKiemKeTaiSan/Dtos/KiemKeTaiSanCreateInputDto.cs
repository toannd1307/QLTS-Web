namespace MyProject.QuanLyKiemKeTaiSan.Dtos
{
    using System;
    using System.Collections.Generic;
    using Abp.Application.Services.Dto;
    using Abp.AutoMapper;
    using DbEntities;

    [AutoMap(typeof(KiemKeTaiSan))]
    public class KiemKeTaiSanCreateInputDto : EntityDto<int>
    {
        public string MaKiemKe { get; set; }

        public string TenKiemKe { get; set; }

        public DateTime? ThoiGianBatDauDuKien { get; set; }

        public DateTime? ThoiGianBatDauThucTe { get; set; }

        public DateTime? ThoiGianKetThucDuKien { get; set; }

        public DateTime? ThoiGianKetThucThucTe { get; set; }

        public int? BoPhanDuocKiemKeId { get; set; }

        public int? TrangThaiId { get; set; }

        public List<long> DoiKiemKeIdList { get; set; }
    }
}