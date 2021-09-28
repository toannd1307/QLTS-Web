﻿// This file is not generated, but this comment is necessary to exclude it from StyleCop analysis 
// <auto-generated/> 
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbEntities
{

    [Table("NhaCungCap_File")]
    public class NhaCungCap_File : FullAuditedEntity, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public virtual int? NhaCungCapId { get; set; }
        public virtual string TenFile { get; set; }
        public virtual string LinkFile { get; set; }
        public virtual int? LoaiFile { get; set; }
        public virtual string GhiChu { get; set; }
    }
}