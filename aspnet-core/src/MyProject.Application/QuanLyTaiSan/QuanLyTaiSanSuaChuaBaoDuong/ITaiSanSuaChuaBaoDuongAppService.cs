namespace MyProject.TaiSanSuaChuaBaoDuong
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using DbEntities;
    using MyProject.Data;
    using MyProject.QuanLyTaiSan.Dtos;

    public interface ITaiSanSuaChuaBaoDuongAppService
    {
        Task<PagedResultDto<ViewTaiSanSuaChuaBaoDuong>> GetAllTaiSanSuaChuaBaoDuongAsync(TaiSanSuaChuaBaoDuongGetAllInputDto input);

        Task<PagedResultDto<ViewTaiSan>> GetAllTaiSanAsync(TaiSanGetAllInputDto input);

        Task<int> EditTaiSanSuaChuaBaoDuong(ViewTaiSanSuaChuaBaoDuong input);

        Task<int> CreateTaiSanSuaChuaBaoDuong(PhieuTaiSanCreateInputDto input);

        Task HoanTacTaiSanSuaChuaBaoDuongAsync(EntityDto[] input);
    }
}
