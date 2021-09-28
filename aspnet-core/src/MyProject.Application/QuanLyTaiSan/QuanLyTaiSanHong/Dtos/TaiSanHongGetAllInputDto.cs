namespace MyProject.QuanLyTaiSan.Dtos
{
    using Abp.Application.Services.Dto;
    using System.Collections.Generic;

    public class TaiSanHongGetAllInputDto : PagedAndSortedResultRequestDto
    {
        public string TenTaiSan { get; set; }

        public List<int> PhongBanqQL { get; set; }

        public int? LoaiTaiSan { get; set; }

        public string NhaCungCap { get; set; }

        public bool? IsSearch { get; set; }
    }
}
