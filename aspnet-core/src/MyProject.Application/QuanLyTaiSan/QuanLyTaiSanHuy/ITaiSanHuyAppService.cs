namespace MyProject.TaiSanHuy
{
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using DbEntities;
    using MyProject.QuanLyTaiSan.Dtos;

    public interface ITaiSanHuyAppService
    {
        Task<PagedResultDto<ViewTaiSanHuy>> GetAllTaiSanHuyAsync(TaiSanHuyGetAllInputDto input);

        Task<PagedResultDto<ViewTaiSan>> GetAllTaiSanAsync(TaiSanGetAllInputDto input);

        Task<int> CreateTaiSanHuy(PhieuTaiSanCreateInputDto input);

        Task HoanTacTaiSanHuyAsync(EntityDto[] input);
    }
}
