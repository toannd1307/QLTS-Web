namespace MyProject.QuanLyLoaiTaiSan
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using MyProject.Data;
    using MyProject.QuanLyLoaiTaiSan.Dtos;

    public interface ILoaiTaiSanAppService
    {
        Task<List<LoaiTaiSanTreeTableForViewDto>> GetAllAsync(LoaiTaiSanGetAllInputDto input);

        Task<int> CreateOrEdit(LoaiTaiSanCreateInputDto input);

        Task<LoaiTaiSanCreateInputDto> GetForEditAsync(EntityDto input, bool isView);

        Task<int> DeleteAsync(EntityDto input);

        Task<FileDto> ExportToExcel(LoaiTaiSanGetAllInputDto input);

        Task<string> ImportFileExcel(string filePath);

        Task<FileDto> DownloadFileMau();
    }
}
