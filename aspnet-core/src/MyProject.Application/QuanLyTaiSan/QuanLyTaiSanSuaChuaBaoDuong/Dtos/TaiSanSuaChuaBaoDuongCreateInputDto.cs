namespace MyProject.QuanLyTaiSan.Dtos
{
    using Abp.Application.Services.Dto;
    using Abp.AutoMapper;
    using DbEntities;

    [AutoMap(typeof(TaiSan))]
    public class TaiSanSuaChuaBaoDuongCreateInputDto : EntityDto<int?>
    {
    }
}
