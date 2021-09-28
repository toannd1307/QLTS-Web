namespace MyProject.TaiSanMat
{
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using DbEntities;
    using MyProject.QuanLyTaiSan.Dtos;

    public interface ITaiSanMatAppService
    {
        Task<PagedResultDto<ViewTaiSanMat>> GetAllTaiSanMatAsync(TaiSanMatGetAllInputDto input);

        Task<PagedResultDto<ViewTaiSan>> GetAllTaiSanAsync(TaiSanGetAllInputDto input);

        Task<int> CreateTaiSanMat(PhieuTaiSanCreateInputDto input);

        Task HoanTacTaiSanMatAsync(EntityDto[] input);

    }
}
