namespace MyProject.QuanLyTaiSan.QuanLyTaiSanChuaSuDung.Dtos
{
    using System;
    using System.Collections.Generic;

    public class TaiSanCapPhatInputDto
    {
        public int PhanLoaiId { get; set; }

        public DateTime NgayKhaiBao { get; set; }

        public int ToChucDuocNhanId { get; set; }

        public string NoiDung { get; set; }

        public List<long> TaiSanIds { get; set; }
    }
}
