namespace MyProject.QuanLyTaiSan.Dtos
{
    using System.Collections.Generic;
    using Abp.Application.Services.Dto;

    public class TaiSanSuaChuaBaoDuongGetAllInputDto : PagedAndSortedResultRequestDto
    {
        public string TenTaiSan { get; set; }

        public List<int> PhongBanqQL { get; set; }

        public int? LoaiTaiSan { get; set; }

        public string NhaCungCap { get; set; }

        public int? HinhThuc { get; set; }

        public int? TrangThai { get; set; }

        public bool? IsSearch { get; set; }
    }
}
