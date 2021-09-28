namespace MyProject.QuanLyNhaCungCap
{
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using MyProject.Data;
    using MyProject.QuanLyNhaCungCap.Dtos;

    public interface INhaCungCapAppService
    {
        Task<PagedResultDto<NhaCungCapForViewDto>> GetAllAsync(NhaCungCapGetAllInputDto input);

        Task<int> CreateOrEdit(NhaCungCapCreateInputDto input);

        Task<NhaCungCapCreateInputDto> GetForEditAsync(EntityDto input, bool isView);

        Task DeleteAsync(EntityDto input);

        Task<FileDto> ExportToExcel(NhaCungCapGetAllInputDto input);

        Task<string> ImportFileExcel(string filePath);

        Task<FileDto> DownloadFileMau();

        bool IsEmail(string inputEmail);
    }
}
