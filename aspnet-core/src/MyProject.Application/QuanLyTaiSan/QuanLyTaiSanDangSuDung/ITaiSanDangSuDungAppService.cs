using Abp.Application.Services.Dto;
using MyProject.QuanLyTaiSan.QuanLyTaiSanChuaSuDung.Dtos;
using System.Threading.Tasks;

namespace MyProject.TaiSanDangSuDung
{
    public interface ITaiSanDangSuDungAppService
    {
        Task<PagedResultDto<TaiSanChuaSuDungForViewDto>> GetAllAsync(TaiSanChuaSuDungGetAllInputDto input);
    }
}
