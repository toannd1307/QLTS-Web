using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using MyProject.QuanLyPhieuDuTruMuaSam.Dtos;

namespace MyProject.QuanLyPhieuDuTruMuaSam
{
    public interface IPhieuDuTruMuaSamAppService
    {
        Task<PagedResultDto<DuTruMuaSamOutPut>> GetAll(DuTruMuaSamInput input);

        void HoanThanh(int input);
    }
}
