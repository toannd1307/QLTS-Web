namespace MyProject.QuanLyToChuc.Dtos
{
    using System.Collections.Generic;

    public class ToChucTreeTableForViewDto
    {
        public ToChucForViewDto Data { get; set; }

        public List<ToChucTreeTableForViewDto> Children { get; set; }

        public bool Expanded { get; set; }
    }
}
