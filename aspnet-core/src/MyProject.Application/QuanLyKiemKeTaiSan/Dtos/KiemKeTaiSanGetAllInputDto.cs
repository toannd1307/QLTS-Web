namespace MyProject.QuanLyKiemKeTaiSan.Dtos
{
    using System;
    using System.Collections.Generic;
    using Abp.Application.Services.Dto;

    public class KiemKeTaiSanGetAllInputDto : PagedAndSortedResultRequestDto
    {
        public string Keyword { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public List<int> BoPhanDuocKiemKeId { get; set; }

        public int? TrangThaiId { get; set; }

        public bool? IsSearch { get; set; }
    }
}
