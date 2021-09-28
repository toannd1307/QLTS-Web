namespace MyProject.QuanLyCanhBao
{
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using MyProject.Data;
    using MyProject.QuanLyCanhBao.Dtos;

    public interface ICanhBaoAppService
    {
        Task<PagedResultDto<CanhBaoForViewDto>> GetAllAsync(CanhBaoInputDto input);
    }
}
