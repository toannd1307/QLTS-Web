namespace MyProject.QuanLyNhaCungCap.Dtos
{
    using Abp.Application.Services.Dto;
    using Abp.AutoMapper;
    using DbEntities;

    [AutoMap(typeof(NhaCungCap))]
    public class NhaCungCapDto : EntityDto<int>
    {
        public string MaNhaCungCap { get; set; }

        public string TenNhaCungCap { get; set; }

        public string DiaChi { get; set; }

        public string SoDienThoai { get; set; }

        public string Email { get; set; }

        public string GhiChu { get; set; }
    }
}