namespace MyProject.QuanLyTaiSan.QuanLyToanBoTaiSan.Dtos
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using DbEntities;

    public class GetForViewDto
    {
        public int? Id { get; set; }

        public string TenTS { get; set; }

        public int? LoaiTS { get; set; }

        public string LoaiTaiSan { get; set; }

        public string SerialNumber { get; set; }

        public string ProductNumber { get; set; }

        public string ReaderMacId { get; set; }

        public int? NhaCC { get; set; }

        public string HangSanXuat { get; set; }

        public string NguyenGia { get; set; }

        public DateTime? NgayMua { get; set; }

        public DateTime? NgayBaoHanh { get; set; }

        public DateTime? HanSD { get; set; }

        public string GhiChu { get; set; }

        public string NguoiChotGia { get; set; }

        public DateTime? ThoiDiemChotGia { get; set; }

        public double GiaCuoiTS { get; set; }

        public string NoiDungChotGia { get; set; }

        public string MaRFID { get; set; }

        public string TinhTrangMaRFID { get; set; }

        public string MaQRCode { get; set; }

        public string TinhTrangMaQRCode { get; set; }

        public string MaBarCode { get; set; }

        public string TinhTrangMaBarCode { get; set; }

        public string EPCCode { get; set; }

        public string MaQuanLy { get; set; }

        public bool? TinhTrangRFID { get; set; }

        public bool? TinhTrangQRCode { get; set; }

        public bool? TinhTrangBarCode { get; set; }

        public string DropdownMultiple { get; set; }

        public int? ThoiGianChietKhauHao { get; set; }

        public int? NguonKinhPhiId { get; set; }

        public List<TaiSanDinhKemFile> ListHinhAnh { get; set; }

        public List<TaiSanDinhKemFile> ListFile { get; set; }

        public List<ViTriTSDto> ListViTriTS { get; set; }

        public List<ThongTinSDDto> ListThongTinSD { get; set; }

        public List<ThongTinSCBDDto> ListThongTinSCBD { get; set; }

        public List<ThongTinHongDto> ListThongTinHong { get; set; }

        public List<ThongTinMatDto> ListThongTinMat { get; set; }

        public List<ThongTinHuyDto> ListThongTinHuy { get; set; }

        public List<ThongTinThanhLyDto> ListThongTinThanhLy { get; set; }
    }
}
