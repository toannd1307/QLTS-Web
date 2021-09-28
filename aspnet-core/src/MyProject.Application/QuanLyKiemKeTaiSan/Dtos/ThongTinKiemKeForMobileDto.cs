namespace MyProject.QuanLyKiemKeTaiSan.Dtos
{
    using System;
    using System.Collections.Generic;

    public class ThongTinKiemKeForMobileDto
    {
        public long KiemKeId { get; set; }

        public DateTime NgayKiemKe { get; set; }

        public string PhongBanKiemKe { get; set; }

        public int TrangThaiKiemKe { get; set; }

        public List<string> ThanhVienKiemKe { get; set; }

        public int ViTriDiaLyId { get; set; }
    }
}
