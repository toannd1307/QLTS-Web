namespace MyProject.QuanLyViTriDiaLy
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Text;
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using Abp.Domain.Repositories;
    using Abp.Linq.Extensions;
    using Abp.UI;
    using DbEntities;
    using Microsoft.EntityFrameworkCore;
    using MyProject.Data;
    using MyProject.Data.Excel.Dtos;
    using MyProject.Global;
    using MyProject.Global.Dtos;
    using MyProject.Net.MimeTypes;
    using MyProject.QuanLyViTriDiaLy.Dtos;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class ViTriDiaLyAppService : MyProjectAppServiceBase, IViTriDiaLyAppService
    {
        private readonly IRepository<ViTriDiaLy> viTriRepository;
        private readonly IRepository<TinhThanh> tinhThanhRepository;
        private readonly IRepository<QuanHuyen> quanHuyenRepository;
        private readonly IRepository<ToChuc> toChucRepository;
        private readonly IAppFolders appFolders;

        public ViTriDiaLyAppService(
            IRepository<ViTriDiaLy> viTriRepository,
            IRepository<TinhThanh> tinhThanhRepository,
            IRepository<QuanHuyen> quanHuyenRepository,
            IRepository<ToChuc> toChucRepository,
            IAppFolders appFolders)
        {
            this.viTriRepository = viTriRepository;
            this.tinhThanhRepository = tinhThanhRepository;
            this.quanHuyenRepository = quanHuyenRepository;
            this.toChucRepository = toChucRepository;
            this.appFolders = appFolders;
        }

        public async Task<PagedResultDto<GetAllDtos>> GetAll(GetAllInPutDtos input)
        {
            List<GetAllDtos> outputtt = new List<GetAllDtos>();

            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            string fillter = input.Fillter;

            var query = from o in this.viTriRepository.GetAll().WhereIf(!string.IsNullOrEmpty(fillter), e => e.TenViTri.Contains(GlobalFunction.RegexFormat(fillter)))
                                                   .WhereIf(input.QuanHuyen != null, e => e.QuanHuyenId == input.QuanHuyen)
                        from qh in this.quanHuyenRepository.GetAll().Where(w => w.Id == o.QuanHuyenId)
                        from tt in this.tinhThanhRepository.GetAll().Where(w => w.Id == qh.TinhThanhId && (input.TinhThanh != null ? w.Id == input.TinhThanh : input.TinhThanh == null))
                        select new GetAllDtos()
                        {
                            Id = o.Id,
                            TenViTri = o.TenViTri,
                            DiaChi = o.DiaChi,
                            GhiChu = o.GhiChu,
                            QuanHuyen = qh.TenQuanHuyen,
                            TinhThanh = tt.TenTinhThanh,
                            NgayTao = o.CreationTime,
                        };

            int totalCount = await query.CountAsync();
            outputtt = await query.OrderBy(input.Sorting ?? "NgayTao DESC")
                .PageBy(input)
                .ToListAsync();
            return new PagedResultDto<GetAllDtos>(totalCount, outputtt);
        }

        public async Task<int> CreateOrEdit(CreateOrEditDtos input)
        {
            int result = 0;

            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            input.TenViTri = GlobalFunction.RegexFormat(input.TenViTri);
            input.DiaChi = GlobalFunction.RegexFormat(input.DiaChi);
            input.GhiChu = GlobalFunction.RegexFormat(input.GhiChu);

            if (!this.CheckExist(input.TenViTri, input.Id))
            {
                if (input.Id == null)
                {
                    await this.Create(input);
                }
                else
                {
                    await this.Update(input);
                }
            }
            else
            {
                result = -1;
            }

            return result;
        }

        public async Task<GetValueForView> GetForEdit(int input, bool isView)
        {
            var entity = await this.viTriRepository.GetAll().Where(w => w.Id == (int)input).FirstOrDefaultAsync();
            var quanHuyen = await this.quanHuyenRepository.GetAll().Where(w => w.Id == entity.QuanHuyenId).FirstOrDefaultAsync();

            GetValueForView output = new GetValueForView();
            output.TenViTri = entity.TenViTri;
            output.TinhThanh = quanHuyen.TinhThanhId;
            output.QuanHuyen = entity.QuanHuyenId;
            output.DiaChi = entity.DiaChi;
            output.GhiChu = entity.GhiChu;

            return output;
        }

        public async Task<List<LookupTableDto>> GetAllDtoTinhThanhAsync()
        {
            var query = await (from tt in this.tinhThanhRepository.GetAll()
                               select new LookupTableDto
                               {
                                   Id = tt.Id,
                                   DisplayName = tt.TenTinhThanh,
                               }).ToListAsync();
            return query;
        }

        public async Task<List<LookupTableDto>> GetAllDtoQuanHuyenAsync()
        {
            var query = await (from qh in this.quanHuyenRepository.GetAll()
                               select new LookupTableDto
                               {
                                   Id = qh.Id,
                                   DisplayName = qh.TenQuanHuyen,
                               }).ToListAsync();
            return query;
        }

        public async Task<List<LookupTableDto>> GetAllDtoQuanHuyenFromTTAsync(int tinhthanhId)
        {
            var query = await (from qh in this.quanHuyenRepository.GetAll().Where(w => w.TinhThanhId == tinhthanhId)
                               select new LookupTableDto
                               {
                                   Id = qh.Id,
                                   DisplayName = qh.TenQuanHuyen,
                               }).ToListAsync();
            return query;
        }

        public async Task<FileDto> ExportToExcel(GetAllInPutDtos input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            // Lấy danh sách cần xuất excel
            var list = await this.GetAll(input);
            using var package = new ExcelPackage();

            // Add sheet
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("ViTriDiaLy");

            var namedStyle = package.Workbook.Styles.CreateNamedStyle("HyperLink");
            namedStyle.Style.Font.UnderLine = true;
            namedStyle.Style.Font.Color.SetColor(Color.Blue);

            // set header
            worksheet.Cells[1, 1].Value = "Tên vị trí";
            worksheet.Cells[1, 2].Value = "Tỉnh thành";
            worksheet.Cells[1, 3].Value = "Quận/Huyện";
            worksheet.Cells[1, 4].Value = "Địa chỉ";
            worksheet.Cells[1, 5].Value = "Ghi chú";

            // Bôi đậm header
            using (ExcelRange r = worksheet.Cells[1, 1, 1, 5])
            {
                using var f = new Font("Calibri", 12, FontStyle.Bold);
                r.Style.Font.SetFromFont(f);
                r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            }

            // Gan gia tri
            var rowNumber = 2;
            list.Items.ToList().ForEach(item =>
            {
                worksheet.Cells[rowNumber, 1].Value = item.TenViTri;
                worksheet.Cells[rowNumber, 2].Value = item.TinhThanh;
                worksheet.Cells[rowNumber, 3].Value = item.QuanHuyen;
                worksheet.Cells[rowNumber, 4].Value = item.DiaChi;
                worksheet.Cells[rowNumber, 5].Value = item.GhiChu;
                rowNumber++;
            });

            // Cho các ô rộng theo dữ liệu
            worksheet.Cells.AutoFitColumns(0);

            worksheet.PrinterSettings.FitToHeight = 1;

            // Tên file
            var fileName = string.Join(".", new string[] { "ViTriDiaLy", "xlsx" });

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

        public async Task<FileDto> DownloadFileMau()
        {
            string fileName = string.Format("ViTriDiaLyImport.xlsx");
            try
            {
                // _appFolders.DemoFileDownloadFolder : Thư mục chưa file mẫu cần tải
                // _appFolders.TempFileDownloadFolder : Không được sửa
                return await GlobalFunction.DownloadFileMau(fileName, this.appFolders.ViTriDiaLyFileDownloadFolder, this.appFolders.TempFileDownloadFolder);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("Có lỗi: " + ex.Message);
            }
        }

        public async Task<string> ImportFileExcel(string filePath)
        {
            StringBuilder returnMessage = new StringBuilder();
            returnMessage.Append("Kết quả nhập file:");
            ReadFromExcelDto<CreateOrEditDtos> readResult = new ReadFromExcelDto<CreateOrEditDtos>();

            // Không tìm thấy file
            if (!File.Exists(filePath))
            {
                readResult.ResultCode = (int)GlobalConst.ReadExcelResultCodeConst.FileNotFound;
            }

            // Đọc hết file excel
            var data = await GlobalFunction.ReadFromExcel(filePath);

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
                        string ten = GlobalFunction.RegexFormat(data[i][0]);
                        string tinhThanh = GlobalFunction.RegexFormat(data[i][1]);
                        string quanHuyen = GlobalFunction.RegexFormat(data[i][2]);
                        string diaChi = GlobalFunction.RegexFormat(data[i][3]);
                        string ghiChu = GlobalFunction.RegexFormat(data[i][4]);

                        var create = new CreateOrEditDtos();

                        create.TenViTri = ten;
                        var quanHuyennn = this.quanHuyenRepository.GetAll().Where(w => w.TenQuanHuyen == quanHuyen).FirstOrDefault();

                        if (quanHuyennn == null)
                        {
                            // xử lý lỗi k có QH
                        }
                        else
                        {
                            create.QuanHuyen = quanHuyennn.Id;
                        }

                        var tinhThanhhh = this.tinhThanhRepository.GetAll().Where(w => w.TenTinhThanh == tinhThanh).FirstOrDefault();

                        if (tinhThanhhh == null)
                        {
                            // xử lý lỗi k có TT
                        }
                        else
                        {
                            create.TinhThanh = tinhThanhhh.Id;
                        }

                        create.DiaChi = diaChi;
                        create.GhiChu = ghiChu;

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

        public async Task<int> Deleted(int input)
        {
            try
            {
                var chekPB = await this.toChucRepository.FirstOrDefaultAsync(w => w.ViTriDiaLyId == input);
                if (chekPB != null)
                {
                    return 3;
                }

                await this.viTriRepository.DeleteAsync(input);
                return 1;
            }
            catch
            {
                return 2;
            }
        }

        private async Task Create(CreateOrEditDtos input)
        {
            var create = new ViTriDiaLy
            {
                TenViTri = input.TenViTri,
                QuanHuyenId = input.QuanHuyen,
                DiaChi = input.DiaChi,
                GhiChu = input.GhiChu,
            };

            await this.viTriRepository.InsertAsync(create);
        }

        private async Task Update(CreateOrEditDtos input)
        {
            var update = await this.viTriRepository.FirstOrDefaultAsync(f => f.Id == input.Id);

            update.TenViTri = input.TenViTri;
            update.QuanHuyenId = input.QuanHuyen;
            update.DiaChi = input.DiaChi;
            update.GhiChu = input.GhiChu;

            await this.viTriRepository.UpdateAsync(update);
        }

        private bool CheckExist(string ma, int? id)
        {
            ma = GlobalFunction.RegexFormat(ma);

            // Nếu query > 0 thì là bị trùng mã => return true
            var query = this.viTriRepository.GetAll().Where(e => e.TenViTri == ma)
                .WhereIf(id != null, e => e.Id != id).Count();
            return query > 0;
        }
    }
}
