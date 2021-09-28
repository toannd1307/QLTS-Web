namespace MyProject.QuanLyPhieuDuTruMuaSam.Dtos
{
    using System;

    public class DuTruMuaSamOutPut
    {
        public long Id { get; set; }

        public int? ToChucId { get; set; }

        public string TenPhongBan { get; set; }

        public string MaPhieu { get; set; }

        public string TenPhieu { get; set; }

        public int? SoLuongDeXuat { get; set; }

        public double? ChiPhiDeXuat { get; set; }

        public long? NguoiLapPhieuId { get; set; }

        public string NguoiLap { get; set; }

        public string NgayLap { get; set; }

        public DateTime NgayLapDate { get; set; }

        public string NgayCapNhat { get; set; }

        public DateTime? NgayCapNhatDate { get; set; }

        public string TrangThai { get; set; }

        public int? TrangThaiId { get; set; }
    }
}
