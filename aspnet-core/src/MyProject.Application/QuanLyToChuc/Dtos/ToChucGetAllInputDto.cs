namespace MyProject.QuanLyToChuc.Dtos
{
    using Abp.Application.Services.Dto;

    public class ToChucGetAllInputDto
    {
        public string Keyword { get; set; }

        public bool? IsSearch { get; set; }
    }
}
