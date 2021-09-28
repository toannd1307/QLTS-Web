namespace CRM.Auditing.Dto
{
    using System;
    using Abp.Application.Services.Dto;
    using Abp.Extensions;
    using Abp.Runtime.Validation;

    public class GetAuditLogsInput : PagedAndSortedResultRequestDto, IShouldNormalize
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string UserName { get; set; }

        public string ServiceName { get; set; }

        public bool? IsSearch { get; set; }

        public void Normalize()
        {
            if (this.Sorting.IsNullOrWhiteSpace())
            {
                this.Sorting = "ExecutionTime DESC";
            }

            if (this.Sorting.IndexOf("UserName", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                this.Sorting = "User." + this.Sorting;
            }
            else
            {
                this.Sorting = "AuditLog." + this.Sorting;
            }
        }
    }
}