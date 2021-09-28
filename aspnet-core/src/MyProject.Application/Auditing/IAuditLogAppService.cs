namespace CRM.Auditing
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Abp.Application.Services;
    using Abp.Application.Services.Dto;
    using CRM.Auditing.Dto;
    using MyProject.Data;

    public interface IAuditLogAppService : IApplicationService
    {
        //Task<PagedResultDto<AuditLogListDto>> GetAuditLogs(GetAuditLogsInput input);

        //Task<FileDto> GetAuditLogsToExcel(GetAuditLogsInput input);
    }
}