namespace MyProject.QuanLyKiemKeTaiSan.Dtos
{
    using System;
    using System.Collections.Generic;
    using Abp.Application.Services.Dto;
    using Abp.AutoMapper;
    using DbEntities;

    [AutoMap(typeof(KiemKe_KetQuaKiemKe))]
    public class KetQuaKiemKeCreateDto : EntityDto<int>
    {
        public long KiemKeTaiSanId { get; set; }

        public int DauDocId { get; set; }

        public List<string> Code { get; set; }
    }
}