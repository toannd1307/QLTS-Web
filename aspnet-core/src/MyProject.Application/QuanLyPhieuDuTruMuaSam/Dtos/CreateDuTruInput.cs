namespace MyProject.QuanLyPhieuDuTruMuaSam.Dtos
{
    using System.Collections.Generic;
    using Abp.Application.Services.Dto;
    using Abp.AutoMapper;
    using DbEntities;

    [AutoMap(typeof(PhieuDuTruMuaSam))]
    public class CreateDuTruInput : EntityDto<int?>
    {
        public string MaPhieu { get; set; }

        public string TenPhieu { get; set; }

        public int? ToChucId { get; set; }

        public int? NguoiLapPhieuId { get; set; }

        public string NgayLapPhieuStr { get; set; }

        public int? TrangThaiId { get; set; }

        public List<PhieuDuTruMuaSamChiTiet> ListPhieuChiTiet { get; set; }

        public List<PhieuDuTruMuaSamDinhKemFile> ListDinhKem { get; set; }
    }
}
