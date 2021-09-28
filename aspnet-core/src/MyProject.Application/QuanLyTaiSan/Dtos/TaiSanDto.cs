namespace MyProject.QuanLyTaiSan.Dtos
{
    using System;
    using System.Collections.Generic;
    using Abp.Application.Services.Dto;
    using Abp.AutoMapper;
    using DbEntities;

    [AutoMap(typeof(TaiSan))]
    public class TaiSanDto : EntityDto<int>
    {
        public DateTime NgayMua { get; set; }

        public DateTime NgayHetHanSuDung { get; set; }

        public DateTime NgayHetHanBaoHanh { get; set; }

        public long? NguoiChotGiaId { get; set; }

        public DateTime? ThoiDiemChotGia { get; set; }

        public double? GiaCuoi { get; set; }

        public string NoiDungChotGia { get; set; }

        public string EPCCode { get; set; }

        public string QRCode { get; set; }

        public string BarCode { get; set; }

        public string RFIDCode { get; set; }

        public bool? TinhTrangSuDungQRCode { get; set; }

        public bool? TinhTrangSuDungBarCode { get; set; }

        public bool? TinhTrangSuDungRFIDCode { get; set; }

        public int? ToChucId { get; set; }

        public string GhiChu { get; set; }

        public string TenTaiSan { get; set; }

        public int? LoaiTaiSanId { get; set; }

        public string SerialNumber { get; set; }

        public string ProductNumber { get; set; }

        public int? NhaCungCapId { get; set; }

        public string HangSanXuat { get; set; }

        public double? NguyenGia { get; set; }

        public int? TrangThaiId { get; set; }

        public string ReaderMacId { get; set; }

        public List<TaiSanDinhKemFile> TaiSanDinhKemFileList { get; set; }
    }
}
