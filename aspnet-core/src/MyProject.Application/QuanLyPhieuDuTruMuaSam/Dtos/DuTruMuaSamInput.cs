namespace MyProject.QuanLyPhieuDuTruMuaSam.Dtos
{
    using System.Collections.Generic;
    using Abp.Application.Services.Dto;

    public class DuTruMuaSamInput : PagedAndSortedResultRequestDto
    {
        public string Keyword { get; set; }

        public List<int?> PhongBan { get; set; }

        public bool? IsSearch { get; set; }
    }
}
