namespace MyProject.QuanLyTaiSan.Dtos
{
    using System.Collections.Generic;
    using Abp.Application.Services.Dto;

    public class TaiSanGetAllInputDto : PagedAndSortedResultRequestDto
    {
        public string TenTaiSan { get; set; }

        public List<int> PhongBanqQL { get; set; }

        public int? LoaiTaiSan { get; set; }

        public bool? IsSearch { get; set; }
    }
}
