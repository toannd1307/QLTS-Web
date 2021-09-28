namespace MyProject.QuanLyTaiSan.QuanLyTaiSanSuaChuaBaoDuong.Dtos
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class ListBaoCaoChiTietDto
    {
        public DateTime? NgayKhaiBao { get; set; }

        public int? ListDangSuDung { get; set; }

        public int? ListCapPhat { get; set; }

        public int? ListThuHoi { get; set; }

        public int? ListDieuChuyen { get; set; }

        public int? ListBaoMat { get; set; }

        public int? ListBaoHong { get; set; }

        public int? ListBaoHuy { get; set; }

        public int? ListThanhLy { get; set; }

        public int? ListDuTruMuaSam { get; set; }

        public int? ListSuaChua { get; set; }

        public int? ListBaoDuong { get; set; }

        public string ToTal { get; set; }

        public bool isCheck { get; set; }
    }
}
