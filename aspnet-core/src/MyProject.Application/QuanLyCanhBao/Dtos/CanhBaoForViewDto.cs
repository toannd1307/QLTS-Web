namespace MyProject.QuanLyCanhBao.Dtos
{
    using System;

    public class CanhBaoForViewDto
    {
        public int Id { get; set; }

        public string NoiDung { get; set; }

        public int? ToChucId { get; set; }

        public string ToChuc { get; set; }

        public int? TaiKhoanId { get; set; }

        public string ThoiGian { get; set; }

        public DateTime Date { get; set; }
    }
}
