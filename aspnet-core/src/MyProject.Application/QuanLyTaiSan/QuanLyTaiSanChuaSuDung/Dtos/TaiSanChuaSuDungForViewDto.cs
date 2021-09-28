namespace MyProject.QuanLyTaiSan.QuanLyTaiSanChuaSuDung.Dtos
{
    using System;
    using Abp.Domain.Entities.Auditing;

    public class TaiSanChuaSuDungForViewDto : FullAuditedEntity
    {
        public string TenTaiSan { get; set; }

        public string LoaiTaiSan { get; set; }

        public string SerialNumber { get; set; }

        public string NhaCungCap { get; set; }

        public DateTime? NgayMua { get; set; }

        public double? NguyenGia { get; set; }

        public string PhongBanQuanLy { get; set; }

        public int? ToChucId { get; set; }

        public string MaSuDung { get; set; }

        public string ViTriTaiSan { get; set; }

        public DateTime? TimeSort { get; set; }

        public bool? CapPhat { get; set; }

        public bool? DieuChuyen { get; set; }

        public bool? ThuHoi { get; set; }

        public DateTime? NgayKhaiBao { get; set; }

        public string EPCCode { get; set; }
    }
}
