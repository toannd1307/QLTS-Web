namespace MyProject.BaoCaoThongTinThietBiRFID
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MyProject.QuanLyTaiSan.QuanLyTaiSanSuaChuaBaoDuong.Dtos;

    public interface IBaoCaoThongTinThietBiRFIDAppService
    {
        Task<List<ListBaoCaoChiTietDto>> GetAllBaoCao(List<int> phongBanqQL, DateTime? tuNgay, DateTime? denNgay, bool? isSearch);
    }
}
