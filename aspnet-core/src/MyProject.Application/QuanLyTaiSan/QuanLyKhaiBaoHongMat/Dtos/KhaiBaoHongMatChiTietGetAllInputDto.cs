namespace MyProject.QuanLyTaiSan.Dtos
{
    using Abp.Application.Services.Dto;

    public class KhaiBaoHongMatChiTietGetAllInputDto : PagedAndSortedResultRequestDto
    {
        public long Id { get; set; }
    }
}
