namespace MyProject.QuanLyTaiSan.QuanLyToanBoTaiSan.Dtos
{
    using System;
    using System.Collections.Generic;
    using DbEntities;

    public class CreateInputDto
    {
        public int? Id { get; set; }

        public string TenTS { get; set; }

        public int? LoaiTS { get; set; }

        public string SerialNumber { get; set; }

        public string ProductNumber { get; set; }

        public string ReaderMACId { get; set; }

        public int? NhaCC { get; set; }

        public string HangSanXuat { get; set; }

        public float? NguyenGia { get; set; }

        public DateTime? NgayMua { get; set; }

        public DateTime? NgayBaoHanh { get; set; }

        public DateTime? HanSD { get; set; }

        public string GhiChu { get; set; }

        public long? NguoiChotGia { get; set; }

        public DateTime? ThoiDiemChotGia { get; set; }

        public float? GiaCuoiTS { get; set; }

        public string NoiDungChotGia { get; set; }

        public string DropdownMultiple { get; set; }

        public int? ThoiGianChietKhauHao { get; set; }

        public int? NguonKinhPhiId { get; set; }

        public List<TaiSanDinhKemFile> ListHA { get; set; }

        public List<TaiSanDinhKemFile> ListFile { get; set; }
    }
}
