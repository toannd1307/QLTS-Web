namespace MyProject.QuanLyLoaiTaiSan.Dtos
{
    using System.Collections.Generic;

    public class LoaiTaiSanTreeTableForViewDto
    {
        public LoaiTaiSanForViewDto Data { get; set; }

        public List<LoaiTaiSanTreeTableForViewDto> Children { get; set; }

        public bool Expanded { get; set; }
    }
}
