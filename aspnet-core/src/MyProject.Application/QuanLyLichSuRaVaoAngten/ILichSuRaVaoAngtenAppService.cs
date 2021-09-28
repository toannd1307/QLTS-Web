namespace MyProject.QuanLyLichSuRaVaoAngten
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using MyProject.Global.Dtos;
    using MyProject.QuanLyLichSuRaVaoAngten.Dtos;

    public interface ILichSuRaVaoAngtenAppService
    {
        Task<PagedResultDto<LichSuRaVaoForViewDto>> GetAll(InputLichSuRaVaoDto input);

        Task<PagedResultDto<LichSuRaVaoForViewDto>> GetAllTaiSanLa(InputLichSuRaVaoDto input);

        Task<List<LookupTableDto>> GetAllNhaCC();
    }
}
