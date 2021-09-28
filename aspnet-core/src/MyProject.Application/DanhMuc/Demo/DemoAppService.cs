// <copyright file="DemoAppService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MyProject.DanhMuc.Demo
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using Abp.Authorization;
    using Abp.Domain.Repositories;
    using Abp.Linq.Extensions;
    using Abp.UI;
    using DbEntities;
    using Microsoft.EntityFrameworkCore;
    using MyProject.Authorization;
    using MyProject.DanhMuc.Demo.Dtos;
    using MyProject.Data;
    using MyProject.Data.Excel.Dtos;
    using MyProject.Global;
    using MyProject.Net.MimeTypes;
    using Newtonsoft.Json;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class DemoAppService : MyProjectAppServiceBase, IDemoAppService
    {
        private readonly IRepository<Demo> demoRepository;
        private readonly IRepository<Demo_File> demoFileRepository;
        private readonly IAppFolders appFolders;

        public DemoAppService(
           IRepository<Demo> demoRepository,
           IAppFolders appFolders,
           IRepository<Demo_File> demoFileRepository)
        {
            this.demoRepository = demoRepository;
            this.demoFileRepository = demoFileRepository;
            this.appFolders = appFolders;
        }

        /// <summary>
        /// Lấy danh sách.
        /// </summary>
        /// <param name="input">Điều kiện tìm kiếm.</param>
        /// <returns>Danh sách.</returns>
        public async Task<PagedResultDto<DemoForView>> GetAllAsync(DemoGetAllInputDto input)
        {
            var filter = this.demoRepository.GetAll()
                        .WhereIf(input != null && !string.IsNullOrEmpty(input.Keyword), e => e.Ma.Contains(input.Keyword) || e.Ten.Contains(input.Keyword));
            var totalCount = await filter.CountAsync();
            var query = from o in filter
                        select new DemoForView()
                        {
                            Demo = this.ObjectMapper.Map<DemoDto>(o),
                            TrangThai = GlobalModel.TrangThaiHieuLucSorted.ContainsKey((int)o.DropdownSingle) ? GlobalModel.TrangThaiHieuLucSorted[(int)o.DropdownSingle] : string.Empty,
                            TrangThaiDuyet = GlobalModel.TrangThaiDuyetSorted.ContainsKey((int)o.AutoCompleteSingle) ? GlobalModel.TrangThaiDuyetSorted[(int)o.AutoCompleteSingle] : string.Empty,
                        };
            var items = query.PageBy(input).ToList();
            return new PagedResultDto<DemoForView>
            {
                TotalCount = totalCount,
                Items = items,
            };
        }

        /// <summary>
        /// Kiểm tra thêm mới hay cập nhật.
        /// </summary>
        /// <param name="input">Đầu vào.</param>
        public async Task<int> CreateOrEdit(DemoCreateInput input)
        {
            // check null input
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }

            input.Ma = GlobalFunction.RegexFormat(input.Ma);
            input.Ten = GlobalFunction.RegexFormat(input.Ten);
            input.DateBasic = GlobalFunction.GetDateTime(input.DateBasic);
            input.DateTime = GlobalFunction.GetDateTime(input.DateTime);
            input.DateDisable = GlobalFunction.GetDateTime(input.DateDisable);
            input.DateMinMax = GlobalFunction.GetDateTime(input.DateMinMax);
            input.DateFrom = GlobalFunction.GetDateTime(input.DateFrom);
            input.DateTo = GlobalFunction.GetDateTime(input.DateTo);
            input.DateMultipleMonth = GlobalFunction.GetDateTime(input.DateMultipleMonth);
            input.MonthOnly = GlobalFunction.GetDateTime(input.MonthOnly);

            if (this.CheckExist(input.Ma, input.Id))
            {
                return 1;
            }

            // nếu là thêm mới
            if (input.Id == null)
            {
                await this.Create(input);
            }
            else
            {
                // là cập nhật
                await this.Update(input);
            }

            return 0;
        }

        /// <summary>
        /// Lấy dữ liệu để cập nhật.
        /// </summary>
        /// <param name="input">Id cần cập nhật.</param>
        /// <returns>Dữ liệu để hiển thị lên form.</returns>
        public async Task<DemoCreateInput> GetForEditAsync(EntityDto input)
        {
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }

            var demo = this.demoRepository.GetAllIncluding(e => e.ListDemoFile).First(e => e.Id == (int)input.Id);
            var edit = this.ObjectMapper.Map<DemoCreateInput>(demo);
            return await Task.FromResult(edit);
        }

        /// <summary>
        /// Xóa.
        /// </summary>
        /// <param name="input">Id cần xóa.</param>
        public async Task Delete(EntityDto input)
        {
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }

            await this.demoRepository.DeleteAsync(input.Id);
        }

        /// <summary>
        /// Xuất excel.
        /// </summary>
        /// <param name="input">Điều kiện xuất excel.</param>
        /// <returns>File excel.</returns>
        public async Task<FileDto> ExportToExcel(DemoGetAllInputDto input)
        {
            // Lấy danh sách cần xuất excel
            var list = await this.GetAllAsync(input);
            using (var package = new ExcelPackage())
            {
                // Add sheet
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Demo");

                var namedStyle = package.Workbook.Styles.CreateNamedStyle("HyperLink");
                namedStyle.Style.Font.UnderLine = true;
                namedStyle.Style.Font.Color.SetColor(Color.Blue);

                // set header
                worksheet.Cells[1, 1].Value = "Mã";
                worksheet.Cells[1, 2].Value = "Tên";

                // Bôi đậm header
                using (ExcelRange r = worksheet.Cells[1, 1, 1, 2])
                {
                    using (var f = new Font("Calibri", 12, FontStyle.Bold))
                    {
                        r.Style.Font.SetFromFont(f);
                        r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                }

                // Gan gia tri
                var rowNumber = 2;
                list.Items.ToList().ForEach(item =>
                {
                    worksheet.Cells[rowNumber, 1].Value = item.Demo.Ma;
                    worksheet.Cells[rowNumber, 2].Value = item.Demo.Ten;
                    rowNumber++;
                });

                // Cho các ô rộng theo dữ liệu
                worksheet.Cells.AutoFitColumns(0);

                worksheet.PrinterSettings.FitToHeight = 1;

                // Tên file
                var fileName = string.Join(".", new string[] { "Demo", "xlsx" });

                // Lưu file vào server
                using (var stream = new MemoryStream())
                {
                    package.SaveAs(stream);
                }

                var file = new FileDto(fileName, MimeTypeNames.ApplicationVndOpenxmlformatsOfficedocumentSpreadsheetmlSheet);
                var filePath = Path.Combine(this.appFolders.TempFileDownloadFolder, file.FileToken);
                package.SaveAs(new FileInfo(filePath));
                return file;
            }
        }

        /// <summary>
        /// Import excel.
        /// </summary>
        /// <param name="FilePath">Đường dẫn file trên server.</param>
        /// <returns>Danh sách đường dẫn.</returns>
        public async Task<string> ImportFileExcel(string FilePath)
        {
            StringBuilder returnMessage = new StringBuilder();
            returnMessage.Append("Kết quả nhập file:");
            ReadFromExcelDto<DemoCreateInput> readResult = new ReadFromExcelDto<DemoCreateInput>();

            // Không tìm thấy file
            if (!File.Exists(FilePath))
            {
                readResult.ResultCode = (int)GlobalConst.ReadExcelResultCodeConst.FileNotFound;
            }

            // Đọc hết file excel
            var data = await GlobalFunction.ReadFromExcel(FilePath);

            // Không có dữ liệu
            if (data.Count <= 0)
            {
                readResult.ResultCode = (int)GlobalConst.ReadExcelResultCodeConst.CantReadData;
            }
            else
            {
                // Đọc lần lượt từng dòng
                for (int i = 0; i < data.Count; i++)
                {
                    try
                    {
                        string ma = data[i][0];
                        string ten = data[i][1];
                        var create = new DemoCreateInput
                        {
                            Ma = ma,
                            Ten = ten,
                        };

                        // Nếu không bị trùng
                        if (await this.CreateOrEdit(create) == 0)
                        {
                            // Đánh dấu các bản ghi thêm thành công
                            readResult.ListResult.Add(create);
                        }
                        else
                        {
                            // Đánh dấu các bản ghi lỗi
                            readResult.ListErrorRow.Add(data[i]);
                            readResult.ListErrorRowIndex.Add(i + 1);
                        }
                    }
                    catch
                    {
                        // Đánh dấu các bản ghi lỗi
                        readResult.ListErrorRow.Add(data[i]);
                        readResult.ListErrorRowIndex.Add(i + 1);
                    }
                }
            }

            // Thông tin import
            readResult.ErrorMessage = GlobalModel.ReadExcelResultCodeSorted[readResult.ResultCode];

            // Nếu đọc file thất bại
            if (readResult.ResultCode != 200)
            {
                return readResult.ErrorMessage;
            }
            else
            {
                // Đọc file thành công
                // Trả kết quả import
                returnMessage.Append(string.Format("\r\n\u00A0- Tổng ghi: {0}", readResult.ListResult.Count + readResult.ListErrorRow.Count));
                returnMessage.Append(string.Format("\r\n\u00A0- Số bản ghi thành công: {0}", readResult.ListResult.Count));
                returnMessage.Append(string.Format("\r\n\u00A0- Số bản ghi thất bại: {0}", readResult.ListErrorRow.Count));
                if (readResult.ListErrorRowIndex.Count > 0)
                {
                    returnMessage.Append(string.Format("\r\n\u00A0- Các dòng thất bại: {0}", string.Join(",", readResult.ListErrorRowIndex)));
                }
            }

            return returnMessage.ToString();
        }

        /// <summary>
        /// Tải file mẫu cho import.
        /// </summary>
        /// <returns>File mẫu.</returns>
        public async Task<FileDto> DownloadFileMau()
        {
            string fileName = string.Format("DemoImport.xlsx");
            try
            {
                // _appFolders.DemoFileDownloadFolder : Thư mục chưa file mẫu cần tải
                // _appFolders.TempFileDownloadFolder : Không được sửa
                return await GlobalFunction.DownloadFileMau(fileName, this.appFolders.DemoFileDownloadFolder, this.appFolders.TempFileDownloadFolder);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("Có lỗi: " + ex.Message);
            }
        }

        public async Task<FileDto> DownloadFileUpload(string linkFile)
        {
            if (string.IsNullOrEmpty(linkFile))
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }

            var fileName = linkFile.Split(@"\").Last();
            var path = this.appFolders.DemoUploadFolder + linkFile.Replace(fileName, string.Empty);

            // _appFolders.DemoFileDownloadFolder : Thư mục chưa file mẫu cần tải
            // _appFolders.TempFileDownloadFolder : Không được sửa
            return await GlobalFunction.DownloadFileMau(fileName, path, this.appFolders.TempFileDownloadFolder);
        }

        public async Task<FileDto> DownloadIphoneApp()
        {
            string fileName = string.Format("QRCodeIphoneApp.ipa");
            try
            {
                // _appFolders.DemoFileDownloadFolder : Thư mục chưa file mẫu cần tải
                // _appFolders.TempFileDownloadFolder : Không được sửa
                return await GlobalFunction.DownloadFileMau(fileName, this.appFolders.IphoneAppFolder, this.appFolders.TempFileDownloadFolder);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("Có lỗi: " + ex.Message);
            }
        }

        public async Task<FileDto> DownloadAndroidApp()
        {
            string fileName = string.Format("QRCodeAndroidApp.apk");
            try
            {
                // _appFolders.DemoFileDownloadFolder : Thư mục chưa file mẫu cần tải
                // _appFolders.TempFileDownloadFolder : Không được sửa
                return await GlobalFunction.DownloadFileMau(fileName, this.appFolders.AndroidAppFolder, this.appFolders.TempFileDownloadFolder);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("Có lỗi: " + ex.Message);
            }
        }

        private bool CheckExist(string ma, int? id)
        {
            ma = GlobalFunction.RegexFormat(ma);

            // Nếu query > 0 thì là bị trùng mã => return true
            var query = this.demoRepository.GetAll().Where(e => e.Ma == ma)
                .WhereIf(id != null, e => e.Id != id).Count();
            return query > 0;
        }

        private async Task Create(DemoCreateInput input)
        {
            var create = this.ObjectMapper.Map<Demo>(input);
            await this.demoRepository.InsertAndGetIdAsync(create);
        }

        private async Task Update(DemoCreateInput input)
        {
            var update = await this.demoRepository.GetAsync((int)input.Id);
            await this.demoFileRepository.DeleteAsync(w => w.DemoId == input.Id);
            this.ObjectMapper.Map(input, update);
        }
    }
}
