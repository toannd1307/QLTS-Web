namespace MyProject.KhaiBaoHongMat
{
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using DbEntities;
    using MyProject.QuanLyTaiSan.Dtos;

    public interface IKhaiBaoHongMatAppService
    {
        Task<PagedResultDto<ViewKhaiBaoHongMat>> GetAllKhaiBaoHongMatAsync(KhaiBaoHongMatGetAllInputDto input);
    }
}
