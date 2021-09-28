namespace MyProject.TaiSanChuaSuDung
{
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using MyProject.QuanLyTaiSan.QuanLyTaiSanChuaSuDung.Dtos;

    public interface ITaiSanChuaSuDungAppService
    {
        Task<PagedResultDto<TaiSanChuaSuDungForViewDto>> GetAllAsync(TaiSanChuaSuDungGetAllInputDto input);
    }
}
