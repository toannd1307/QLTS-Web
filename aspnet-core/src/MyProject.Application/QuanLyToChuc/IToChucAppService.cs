namespace MyProject.QuanLyToChuc
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using MyProject.Data;
    using MyProject.Global.Dtos;
    using MyProject.QuanLyToChuc.Dtos;

    public interface IToChucAppService
    {
        Task<List<ToChucTreeTableForViewDto>> GetAllAsync(ToChucGetAllInputDto input);

        Task<int> CreateOrEdit(ToChucCreateInputDto input);

        Task<ToChucCreateInputDto> GetForEditAsync(EntityDto input, bool isView);

        Task DeleteAsync(EntityDto input);

        Task<FileDto> ExportToExcel(ToChucGetAllInputDto input);

        Task<string> ImportFileExcel(string filePath);

        Task<FileDto> DownloadFileMau();

        Task<List<LookupTableDto>> GetAllToChucCha();
    }
}
