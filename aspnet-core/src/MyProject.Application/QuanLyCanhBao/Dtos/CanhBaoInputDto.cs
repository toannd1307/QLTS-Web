namespace MyProject.QuanLyCanhBao.Dtos
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Abp.Application.Services.Dto;

    public class CanhBaoInputDto : PagedAndSortedResultRequestDto
    {
        public int? TaiKhoanId { get; set; }

        public List<int?> ToChucId { get; set; }

        public string NoiDung { get; set; }

        public int? HoatDong { get; set; }

        public DateTime? ThoiGianFrom { get; set; }

        public DateTime? ThoiGianTo { get; set; }

        public bool? IsSearch { get; set; }
    }
}
