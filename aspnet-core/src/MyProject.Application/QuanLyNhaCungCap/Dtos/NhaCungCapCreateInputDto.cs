namespace MyProject.QuanLyNhaCungCap.Dtos
{
    using System.Collections.Generic;
    using Abp.Application.Services.Dto;
    using Abp.AutoMapper;
    using DbEntities;

    [AutoMap(typeof(NhaCungCap))]
    public class NhaCungCapCreateInputDto : EntityDto<int?>
    {
        public string MaNhaCungCap { get; set; }

        public string TenNhaCungCap { get; set; }

        public string DiaChi { get; set; }

        public string SoDienThoai { get; set; }

        public int? LinhVucKinhDoanhId { get; set; }

        public string MaSoThue { get; set; }

        public string Email { get; set; }

        public string GhiChu { get; set; }

        public List<NhaCungCap_File> ListFile { get; set; }
    }
}
