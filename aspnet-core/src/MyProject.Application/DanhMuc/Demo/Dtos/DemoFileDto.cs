namespace MyProject.DanhMuc.Demo.Dtos
{
     using Abp.Application.Services.Dto;
     using Abp.AutoMapper;
     using DbEntities;

     [AutoMap(typeof(Demo_File))]
     public class DemoFileDto : EntityDto<int>
     {
          public int? DemoId { get; set; }

          public string TenFile { get; set; }

          public string LinkFile { get; set; }

          public string GhiChu { get; set; }
     }
}
