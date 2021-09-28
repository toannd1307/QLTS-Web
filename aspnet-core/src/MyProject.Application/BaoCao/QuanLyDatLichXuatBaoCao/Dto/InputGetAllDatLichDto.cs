using Abp.Application.Services.Dto;

namespace MyProject.BaoCao.QuanLyDatLichXuatBaoCao.Dto
{
    public class InputGetAllDatLichDto : PagedAndSortedResultRequestDto
    {
        public string Fillter { get; set; }

        public bool? IsSearch { get; set; }
    }
}