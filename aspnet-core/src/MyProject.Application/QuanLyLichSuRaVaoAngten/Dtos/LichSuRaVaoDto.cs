namespace MyProject.QuanLyLichSuRaVaoAngten.Dtos
{
    using System;
    using Abp.Application.Services.Dto;
    using Abp.AutoMapper;
    using DbEntities;

    [AutoMap(typeof(LichSuRaVaoAngten))]
    public class LichSuRaVaoDto : EntityDto<int>
    {
        public int? TenantId { get; set; }

        public string ReaderMacId { get; set; }

        public string RFID { get; set; }

        public DateTime? Ngay { get; set; }

        public int? ViTri { get; set; }

        public int? AntennaPort { get; set; }

        public int? TaiSanId { get; set; }
    }
}
