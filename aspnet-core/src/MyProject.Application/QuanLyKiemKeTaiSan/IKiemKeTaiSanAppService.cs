namespace MyProject.QuanLyKiemKeTaiSan
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using MyProject.Authorization.Users;
    using MyProject.QuanLyKiemKeTaiSan.Dtos;
    using MyProject.Users.Dto;

    public interface IKiemKeTaiSanAppService
    {
        Task<PagedResultDto<KiemKeTaiSanForViewDto>> GetAllAsync(KiemKeTaiSanGetAllInputDto input);

        Task<int> CreateOrEdit(KiemKeTaiSanCreateInputDto input);

        Task<KiemKeTaiSanCreateInputDto> GetForEditAsync(EntityDto input, bool isView);

        Task<List<UserForViewDto>> GetUserForEditAsync(EntityDto input);

        Task<List<KetQuaKiemKeForViewDto>> GetAllTaiSanAsync(KetQuaKiemKeGetAllInputDto input);

        Task DeleteAsync(List<long> input);

        string CreateKetQuaKiemKe(KetQuaKiemKeCreateDto input);
    }
}
