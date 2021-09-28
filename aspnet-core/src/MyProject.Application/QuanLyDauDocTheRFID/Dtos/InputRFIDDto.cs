namespace MyProject.QuanLyDauDocTheRFID.Dtos
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Abp.Application.Services.Dto;

    public class InputRFIDDto : PagedAndSortedResultRequestDto
    {
        public string TenTS { get; set; }

        public List<int?> PhongBanSuDung { get; set; }

        public int? TinhTrangSuDung { get; set; }

        public bool? IsSearch { get; set; }
    }
}
