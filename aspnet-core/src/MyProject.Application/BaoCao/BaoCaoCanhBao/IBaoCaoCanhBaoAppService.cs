namespace MyProject.BaoCaoCanhBao
{
    using MyProject.BaoCao.BaoCaoCanhBao.Dtos;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IBaoCaoCanhBaoAppService
    {
        List<ListBaoCaoCanhBaoOutputDto> GetAllBaoCao(List<int?> phongBanqQL, DateTime? tuNgay, DateTime? denNgay, bool? isSearch);
    }
}
