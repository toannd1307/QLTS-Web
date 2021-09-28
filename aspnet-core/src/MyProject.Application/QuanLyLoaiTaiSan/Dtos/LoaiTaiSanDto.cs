namespace MyProject.QuanLyLoaiTaiSan.Dtos
{
    using Abp.Application.Services.Dto;
    using Abp.AutoMapper;
    using DbEntities;

    [AutoMap(typeof(LoaiTaiSan))]
    public class LoaiTaiSanDto : EntityDto<int>
    {
        public int? TaiSanChaId { get; set; }

        public string Ma { get; set; }

        public string Ten { get; set; }

        public string GhiChu { get; set; }
    }
}
