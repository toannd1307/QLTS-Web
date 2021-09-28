using System;
using System.Collections.Generic;
using System.Text;

namespace MyProject.BaoCao.QuanLyDatLichXuatBaoCao.Dto
{
    public class GetAllDatLichBCDtos
    {
        public int? Id { get; set; }

        public string TenBaoCao { get; set; }

        public string LapLai { get; set; }

        public string GioGuiBC { get; set; }

        public string NgayGio { get; set; }

        public int? NgayGuiTuan { get; set; }

        public int? NgayGuiThang { get; set; }

        public string NgayGuiNam { get; set; }

        public DateTime NgayTao { get; set; }

        public DateTime? NgayCapNhat { get; set; }
    }
}
