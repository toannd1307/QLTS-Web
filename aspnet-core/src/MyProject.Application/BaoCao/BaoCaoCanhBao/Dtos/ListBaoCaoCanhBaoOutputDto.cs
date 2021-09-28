namespace MyProject.BaoCao.BaoCaoCanhBao.Dtos
{
    using System;

    public class ListBaoCaoCanhBaoOutputDto
    {
        public DateTime? NgayKhaiBao { get; set; }

        public int? TaiSanRa { get; set; }

        public int? TaiSanVao { get; set; }

        public int? BatDauKiemKe { get; set; }

        public int? KetThucKiemKe { get; set; }

        public string ToTal { get; set; }

        public bool IsCheck { get; set; }
    }
}
