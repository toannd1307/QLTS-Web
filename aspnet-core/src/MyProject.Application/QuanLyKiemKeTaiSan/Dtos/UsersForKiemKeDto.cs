namespace MyProject.QuanLyKiemKeTaiSan.Dtos
{
    using System;
    using System.Collections.Generic;
    using Abp.Application.Services.Dto;
    using Abp.AutoMapper;
    using DbEntities;

    public class UsersForKiemKeDto : PagedAndSortedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}