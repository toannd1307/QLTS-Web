﻿// This file is not generated, but this comment is necessary to exclude it from StyleCop analysis
// <auto-generated/>
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DbEntities
{
    [Table("LinhVucKinhDoanh")]
    public class LinhVucKinhDoanh : FullAuditedEntity, IMayHaveTenant
    {
        public virtual int? TenantId { get; set; }

        public virtual string TenLinhVuc { get; set; }
    }
}
