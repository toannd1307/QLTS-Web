namespace MyProject.TaiSanThanhLy
{
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using DbEntities;
    using MyProject.QuanLyTaiSan.Dtos;

    public interface ITaiSanThanhLyAppService
    {
        Task<PagedResultDto<ViewTaiSanThanhLy>> GetAllTaiSanThanhLyAsync(TaiSanThanhLyGetAllInputDto input);

        Task<PagedResultDto<ViewTaiSan>> GetAllTaiSanAsync(TaiSanGetAllInputDto input);

        Task<int> CreateTaiSanThanhLy(PhieuTaiSanCreateInputDto input);

        Task HoanTacTaiSanThanhLyAsync(EntityDto[] input);
    }
}
