
namespace MyProject.QuanLyTaiSan.QuanLyToanBoTaiSan.Dtos
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class GetAllOutputDto
    {
        public int Id { get; set; }

        public string TenTS { get; set; }

        public string LoaiTS { get; set; }

        public string SerialNumber { get; set; }

        public string ProductNumber { get; set; }

        public string NhaCungCap { get; set; }

        public string NgayMua { get; set; }

        public DateTime? NgayMuaDateTime { get; set; }

        public double? NguyenGia { get; set; }

        public string PhongBanQL { get; set; }

        public string TinhTrangSDRFID { get; set; }

        public string TinhTrangSDBarCode { get; set; }

        public string TinhTrangSDQRCode { get; set; }

        public string MaSD { get; set; }

        public string ViTriTS { get; set; }

        public DateTime? NgayTao { get; set; }

        public string TrangThai { get; set; }

        public string TinhTrangSuDung { get; set; }

        public int? TrangThaiId { get; set; }

        public bool? TinhTrangRFID { get; set; }

        public bool? TinhTrangQRCode { get; set; }

        public bool? TinhTrangBarCode { get; set; }

        public string MaEPC { get; set; }
        public int? NguonKinhPhiId { get; set; }
    }
}
