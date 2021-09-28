namespace MyProject.QuanLyTaiSan.Dtos
{
    using Abp.Application.Services.Dto;
    using System.Collections.Generic;

    public class KhaiBaoHongMatGetAllInputDto : PagedAndSortedResultRequestDto
    {
        public string TimKiemKhaiBao { get; set; }

        public List<int> PhongBanqQL { get; set; }

        public int? KhaiBao { get; set; }

        public bool? IsSearch { get; set; }
    }
}
