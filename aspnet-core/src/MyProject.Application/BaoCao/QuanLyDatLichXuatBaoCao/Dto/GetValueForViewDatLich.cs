using System;
using System.Collections.Generic;
using System.Text;

namespace MyProject.BaoCao.QuanLyDatLichXuatBaoCao.Dto
{
    public class GetValueForViewDatLich
    {
        public int BaoCaoId { get; set; }

        public string TenBaoCao { get; set; }

        public int? LapLaiId { get; set; }

        public DateTime GioBaoCao { get; set; }

        public string NgayGuiBaoCao { get; set; }

        public int PhongBanNhan { get; set; }

        public string NguoiNhan { get; set; }

        public string GhiChu { get; set; }
    }
}
