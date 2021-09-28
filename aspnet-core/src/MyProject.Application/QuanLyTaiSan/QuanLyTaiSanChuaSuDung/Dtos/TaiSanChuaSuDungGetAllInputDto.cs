namespace MyProject.QuanLyTaiSan.QuanLyTaiSanChuaSuDung.Dtos
{
    using Abp.Application.Services.Dto;
    using System.Collections.Generic;

    public class TaiSanChuaSuDungGetAllInputDto : PagedAndSortedResultRequestDto
    {
        public string KeyWord { get; set; }

        public List<int> PhongBanQuanLyId { get; set; }

        public int? LoaiTaiSanId { get; set; }

        public int? NhaCungCapId { get; set; }

        public int? TinhTrangSuDung { get; set; }

        public bool? IsSearch { get; set; }
    }
}