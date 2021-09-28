namespace MyProject.BaoCaoNguoiDung
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MyProject.BaoCao.BaoCaoNguoiDung.Dtos;

    public interface IBaoCaoNguoiDungAppService
    {
        Task<List<ListViewDto>> GetAllBaoCao(List<int> phongBanqQL, DateTime? tuNgay, DateTime? denNgay, bool? isSearch);
    }
}
