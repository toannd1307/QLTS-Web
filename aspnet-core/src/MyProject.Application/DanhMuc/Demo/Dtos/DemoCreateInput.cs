// This file is not generated, but this comment is necessary to exclude it from StyleCop analysis 
// <auto-generated/> 
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using DbEntities;
using System;
using System.Collections.Generic;

namespace MyProject.DanhMuc.Demo.Dtos
{
     [AutoMap(typeof(Demo))]
     public class DemoCreateInput : EntityDto<int?>
     {
          public string Ma { get; set; }
          public string Ten { get; set; }
          public string Checkbox { get; set; }
          public bool? CheckboxTrueFalse { get; set; }
          public int? RadioButton { get; set; }
          public bool? InputSwitch { get; set; }
          public string InputMask { get; set; }
          public int? Slider { get; set; }
          public string Description { get; set; }
          public string InputTextarea { get; set; }
          public int? IntegerOnly { get; set; }
          public double? Decimal { get; set; }
          public DateTime? DateBasic { get; set; }
          public DateTime? DateTime { get; set; }
          public DateTime? DateDisable { get; set; }
          public DateTime? DateMinMax { get; set; }
          public DateTime? DateFrom { get; set; }
          public DateTime? DateTo { get; set; }
          public string DateMultiple { get; set; }
          public DateTime? DateMultipleMonth { get; set; }
          public DateTime? MonthOnly { get; set; }
          public string TimeOnly { get; set; }
          public int? AutoCompleteSingle { get; set; }
          public string AutoCompleteMultiple { get; set; }
          public int? DropdownSingle { get; set; }
          public string DropdownMultiple { get; set; }
          public List<Demo_File> ListDemoFile { get; set; }
     }
}
