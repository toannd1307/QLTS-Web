namespace MyProject.QuanLyKiemKeTaiSan.Dtos
{
    using Abp.Application.Services.Dto;
    using Abp.AutoMapper;
    using DbEntities;

    [AutoMap(typeof(KiemKe_KetQuaKiemKe))]
    public class KetQuaKiemKeDto : EntityDto<int>
    {
        public int? TenantId { get; set; }

        public long? KiemKeTaiSanId { get; set; }

        public int? DauDocId { get; set; }

        public int? TaiSanId { get; set; }

        public int? ViTriDiaLyId { get; set; }

        public int? TrangThaiTaiSanId { get; set; }

        public int? KetQua { get; set; }

        public string GhiChu { get; set; }
    }
}
