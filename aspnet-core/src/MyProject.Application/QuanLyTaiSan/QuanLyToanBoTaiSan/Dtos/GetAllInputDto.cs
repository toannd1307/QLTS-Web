
namespace MyProject.QuanLyTaiSan.QuanLyToanBoTaiSan.Dtos
{
    using Abp.Application.Services.Dto;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class GetAllInputDto : PagedAndSortedResultRequestDto
    {
        public string Fillter { get; set; }

        public List<int> PhongBanqQL { get; set; }

        public int? LoaiTS { get; set; }

        public int? NhaCungCap { get; set; }

        public int? TinhTrangSD { get; set; }

        public int? MaSD { get; set; }

        public bool? IsSearch { get; set; }
    }
}
