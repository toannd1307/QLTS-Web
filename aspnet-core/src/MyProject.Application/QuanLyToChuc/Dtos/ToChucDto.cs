namespace MyProject.QuanLyToChuc.Dtos
{
    using Abp.Application.Services.Dto;
    using Abp.AutoMapper;
    using DbEntities;

    [AutoMap(typeof(ToChuc))]
    public class ToChucDto : EntityDto<int>
    {
        public string MaToChuc { get; set; }

        public string TenToChuc { get; set; }

        public int? TrucThuocToChucId { get; set; }

        public int? ViTriDiaLyId { get; set; }

        public string GhiChu { get; set; }
    }
}
