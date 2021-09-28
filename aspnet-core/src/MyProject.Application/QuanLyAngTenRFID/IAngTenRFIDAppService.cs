namespace MyProject.QuanLyAngTenRFID
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using MyProject.Data;
    using MyProject.Global.Dtos;
    using MyProject.QuanLyDauDocTheRFID.Dtos;
    using MyProject.QuanLyTaiSan.QuanLyToanBoTaiSan.Dtos;

    public interface IAngTenRFIDAppService
    {
        Task<PagedResultDto<GetAllOutputDto>> GetAll(InputRFIDDto input);

        Task<List<LookupTableDto>> GetAllNhaCC();

        Task<List<LookupTableDto>> GetAllLoaiTS();

        Task<int> CreateOrEdit(CreateInputDto input);

        Task<int> Deleted(int input);

        Task<GetForViewDto> GetForEdit(int input, bool isView);

        Task XoaList(List<int> input);

        Task<FileDto> DownloadFileUpload(string linkFile);
    }
}
