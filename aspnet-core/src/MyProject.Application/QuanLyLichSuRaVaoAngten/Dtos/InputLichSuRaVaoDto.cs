namespace MyProject.QuanLyLichSuRaVaoAngten.Dtos
{
    using System;
    using System.Collections.Generic;
    using Abp.Application.Services.Dto;

    public class InputLichSuRaVaoDto : PagedAndSortedResultRequestDto
    {
        public string Keyword { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string ChieuDiChuyen { get; set; }

        public List<int?> BoPhanId { get; set; }

        public int? PhanLoaiId { get; set; }

        public bool? IsSearch { get; set; }
    }
}
