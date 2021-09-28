namespace MyProject.QuanLyLichSuRaVaoAngten.Dtos
{
    using System;

    public class LichSuRaVaoForViewDto
    {
        public long Id { get; set; }

        public string MaRFID { get; set; }

        public string TenTaiSan { get; set; }

        public int TaiSanId { get; set; }

        public string DonViSuDung { get; set; }

        public int ToChucId { get; set; }

        public int PhongBanSuDungId { get; set; }

        public string LoaiTaiSan { get; set; }

        public int LoaiTaiSanId { get; set; }

        public int ChieuDiChuyenId { get; set; }

        public string ChieuDiChuyen { get; set; }

        public DateTime NgayDiChuyen { get; set; }

        public string PhanLoai { get; set; }
    }
}