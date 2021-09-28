namespace MyProject.QuanLyViTriDiaLy.Dtos
{
    using Abp.Application.Services.Dto;

    public class GetAllInPutDtos : PagedAndSortedResultRequestDto
    {
        public string Fillter { get; set; }

        public int? TinhThanh { get; set; }

        public int? QuanHuyen { get; set; }

        public bool? IsSearch { get; set; }
    }
}
