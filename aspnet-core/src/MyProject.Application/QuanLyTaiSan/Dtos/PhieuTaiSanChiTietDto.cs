namespace MyProject.QuanLyTaiSan.Dtos
{
    using Abp.Application.Services.Dto;
    using Abp.AutoMapper;
    using DbEntities;

    [AutoMap(typeof(PhieuTaiSanChiTiet))]
    public class PhieuTaiSanChiTietDto : EntityDto<long>
    {
        public long? PhieuTaiSanId { get; set; }

        public int? TaiSanId { get; set; }

        public int? TrangThaiId { get; set; }

        public int? ViTriLapDat { get; set; }

        public string GhiChu { get; set; }
    }
}
