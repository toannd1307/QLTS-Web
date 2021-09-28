namespace MyProject.QuanLyViTriDiaLy.Dtos
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class GetAllDtos
    {
        public int Id { get; set; }
        public string TenViTri { get; set; }

        public string TinhThanh { get; set; }

        public string QuanHuyen { get; set; }

        public string DiaChi { get; set; }

        public string GhiChu { get; set; }

        public DateTime NgayTao { get; set; }
    }
}
