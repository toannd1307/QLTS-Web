namespace MyProject.BaoCaoNguoiDung
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using Abp.Auditing;
    using Abp.Domain.Repositories;
    using CRM.Auditing;
    using DbEntities;
    using MyProject.Authorization.Users;
    using MyProject.BaoCao.BaoCaoNguoiDung.Dtos;
    using MyProject.Data;
    using MyProject.Global;
    using MyProject.Net.MimeTypes;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using OfficeOpenXml;
    using OfficeOpenXml.Drawing.Chart;
    using OfficeOpenXml.Style;

    public class ThongTinNguoiDung
    {
#pragma warning disable SA1300 // Element should begin with upper-case letter
        public string userNameOrEmailAddress { get; set; }
#pragma warning restore SA1300 // Element should begin with upper-case letter
    }
}
