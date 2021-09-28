namespace MyProject.QuanLyToChuc.Dtos
{
    using System.Collections.Generic;
    using DbEntities;

    public class ToChucForViewDto
    {
        public ToChuc ToChuc { get; set; }

        public string DiaChi { get; set; }

        public int MaHe10 { get; set; }
    }
}
