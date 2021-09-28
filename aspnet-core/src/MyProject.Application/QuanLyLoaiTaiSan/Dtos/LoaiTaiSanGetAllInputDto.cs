namespace MyProject.QuanLyLoaiTaiSan.Dtos
{
    using Abp.Application.Services.Dto;

    public class LoaiTaiSanGetAllInputDto
    {
        public string Keyword { get; set; }

        public bool? IsSearch { get; set; }
    }
}
