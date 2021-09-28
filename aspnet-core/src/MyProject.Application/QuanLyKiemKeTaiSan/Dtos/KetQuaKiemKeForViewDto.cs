namespace MyProject.QuanLyKiemKeTaiSan.Dtos
{
    public class KetQuaKiemKeForViewDto
    {
        public int Id { get; set; }

        public string MaTaiSan { get; set; }

        public string TenTaiSan { get; set; }

        public int TaiSanId { get; set; }

        public string LoaiTaiSan { get; set; }

        public string ViTri { get; set; }

        public string TinhTrang { get; set; }

        public string TrangThai { get; set; }
        public int TrangThaiId { get; set; }
        public int TinhTrangId { get; set; }

    }
}