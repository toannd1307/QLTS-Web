namespace MyProject.QuanLyViTriDiaLy
{
    using Abp.Application.Services.Dto;
    using MyProject.QuanLyViTriDiaLy.Dtos;
    using System.Threading.Tasks;

    public interface IViTriDiaLyAppService
    {
        Task<PagedResultDto<GetAllDtos>> GetAll(GetAllInPutDtos input);
    }
}
