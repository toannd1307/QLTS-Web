namespace MyProject.TaiSanHong
{
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using DbEntities;
    using MyProject.QuanLyTaiSan.Dtos;

    public interface ITaiSanHongAppService
    {
        Task<PagedResultDto<ViewTaiSanHong>> GetAllTaiSanHongAsync(TaiSanHongGetAllInputDto input);

        Task<PagedResultDto<ViewTaiSan>> GetAllTaiSanAsync(TaiSanGetAllInputDto input);

        Task<int> CreateTaiSanHong(PhieuTaiSanCreateInputDto input);

        Task HoanTacTaiSanHongAsync(EntityDto[] input);
    }
}
