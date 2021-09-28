using System;
using System.Collections.Generic;
using System.Text;

namespace MyProject.QuanLyCanhBao.Dtos
{
    public class InputFromServiceDto
    {
        public int? TaiKhoanId { get; set; }

        public int? PhongBanNhanId { get; set; }

        public string NoiDung { get; set; }

        public int? LoaiCanhBao { get; set; }

        public long? IdLichSu { get; set; }
    }
}
