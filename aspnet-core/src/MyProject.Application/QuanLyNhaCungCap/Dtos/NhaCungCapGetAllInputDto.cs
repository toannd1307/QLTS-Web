namespace MyProject.QuanLyNhaCungCap.Dtos
{
    using Abp.Application.Services.Dto;

    public class NhaCungCapGetAllInputDto : PagedAndSortedResultRequestDto
    {
        public string Keyword { get; set; }

        public int? LinhVuc { get; set; }

        public bool? IsSearch { get; set; }
    }
}
