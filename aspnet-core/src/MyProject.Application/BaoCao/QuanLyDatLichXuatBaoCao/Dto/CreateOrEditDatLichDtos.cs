using System;
using Abp.Application.Services.Dto;

namespace MyProject.QuanLyDatLichXuatBaoCao.Dto
{
    public class CreateOrEditDatLichDtos : EntityDto<int?>
    {
        public int BaoCaoId { get; set; }

        public string TenBaoCao { get; set; }

        public int? LapLaiId { get; set; }

        public DateTime GioGuiBaoCao { get; set; }

        public string NgayGuiBaoCao { get; set; }

        public int PhongBanNhanId { get; set; }

        public string NguoiNhanBaoCaoId { get; set; }

        public string GhiChu { get; set; }
    }
}
