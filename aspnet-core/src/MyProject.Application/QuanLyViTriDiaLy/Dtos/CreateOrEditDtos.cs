namespace MyProject.QuanLyViTriDiaLy.Dtos
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class CreateOrEditDtos
    {
        public int? Id { get; set; }

        public string TenViTri { get; set; }

        public int TinhThanh { get; set; }

        public int QuanHuyen { get; set; }

        public string DiaChi { get; set; }

        public string GhiChu { get; set; }
    }
}
