namespace CRM.Auditing.Dto
{
    using System;
    using Abp.Application.Services.Dto;
    using Abp.Events.Bus.Entities;

    public class EntityChangeListDto : EntityDto<long>
    {
        public long? UserId { get; set; }

        public string UserName { get; set; }

        public DateTime ChangeTime { get; set; }

        public string EntityTypeFullName { get; set; }

        public EntityChangeType ChangeType { get; set; }

        public string ChangeTypeName => this.ChangeType.ToString();

        public long EntityChangeSetId { get; set; }
    }
}