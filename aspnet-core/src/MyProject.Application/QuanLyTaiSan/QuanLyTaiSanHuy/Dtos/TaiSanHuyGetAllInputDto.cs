namespace MyProject.QuanLyTaiSan.Dtos
{
    using System.Collections.Generic;
    using Abp.Application.Services.Dto;

    public class TaiSanHuyGetAllInputDto : PagedAndSortedResultRequestDto
    {
        public string TenTaiSan { get; set; }

        public List<int> PhongBanqQL { get; set; }

        public int? LoaiTaiSan { get; set; }

        public string NhaCungCap { get; set; }

        public bool? IsSearch { get; set; }
    }
}
