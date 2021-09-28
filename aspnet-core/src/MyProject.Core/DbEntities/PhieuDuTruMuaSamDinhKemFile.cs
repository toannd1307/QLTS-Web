// This file is not generated, but this comment is necessary to exclude it from StyleCop analysis
// <auto-generated/>
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbEntities
{
    [Table("PhieuDuTruMuaSamDinhKemFile")]
    public class PhieuDuTruMuaSamDinhKemFile : FullAuditedEntity<long>, IMayHaveTenant
    {
        public virtual int? TenantId { get; set; }
        public virtual long? PhieuDuTruMuaSamId { get; set; }
        public virtual string TenFile { get; set; }
        public virtual string LinkFile { get; set; }
        public virtual int? PhanLoaiId { get; set; }
        public virtual string GhiChu { get; set; }
    }
}
