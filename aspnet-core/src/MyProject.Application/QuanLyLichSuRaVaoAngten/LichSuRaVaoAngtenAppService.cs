namespace MyProject.QuanLyLichSuRaVaoAngten
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using Abp.Collections.Extensions;
    using Abp.Domain.Repositories;
    using Abp.Linq.Extensions;
    using Abp.UI;
    using DbEntities;
    using Microsoft.EntityFrameworkCore;
    using MyProject.Authorization;
    using MyProject.Authorization.Users;
    using MyProject.Data;
    using MyProject.Global;
    using MyProject.Global.Dtos;
    using MyProject.Net.MimeTypes;
    using MyProject.QuanLyLichSuRaVaoAngten.Dtos;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class LichSuRaVaoAngtenAppService : MyProjectAppServiceBase, ILichSuRaVaoAngtenAppService
    {
        private readonly IRepository<NhaCungCap> nhaCungCapRepository;
        private readonly IRepository<TaiSan> taiSanRepository;
        private readonly IRepository<LichSuRaVaoAngten, long> lichSuRaVaoRespository;
        private readonly IRepository<ToChuc> toChucRepository;
        private readonly IAppFolders appFolders;

        public LichSuRaVaoAngtenAppService(
            IRepository<NhaCungCap> nhaCungCapRepository,
            IRepository<LoaiTaiSan> loaiTaiSanRepository,
            IRepository<TaiSan> taiSanRepository,
            IRepository<ToChuc> toChucRepository,
            IRepository<LichSuRaVaoAngten, long> lichSuRaVaoRespository,
            IRepository<User, long> userRepository,
            IRepository<ViTriDiaLy> viTriRepository,
            IAppFolders appFolders)
        {
            this.nhaCungCapRepository = nhaCungCapRepository;
            this.taiSanRepository = taiSanRepository;
            this.toChucRepository = toChucRepository;
            this.lichSuRaVaoRespository = lichSuRaVaoRespository;
            this.appFolders = appFolders;
        }

        public async Task<PagedResultDto<LichSuRaVaoForViewDto>> GetAll(InputLichSuRaVaoDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            string keyWord = GlobalFunction.RegexFormat(input.Keyword);
            input.StartDate = GlobalFunction.GetDateTime(input.StartDate);
            input.EndDate = GlobalFunction.GetDateTime(input.EndDate);

            var query = (from lichSu in this.lichSuRaVaoRespository.GetAll()
                         .WhereIf(!string.IsNullOrEmpty(input.ChieuDiChuyen), e => e.Chieu == input.ChieuDiChuyen)
                         .WhereIf(
                             input.PhanLoaiId != null && input.PhanLoaiId == (int)GlobalConst.PhanLoaiTaiSanTrongHeThongConst.TSTrongHeThong,
                             e => e.TaiSanId != null)
                         .WhereIf(
                             input.PhanLoaiId != null && input.PhanLoaiId == (int)GlobalConst.PhanLoaiTaiSanTrongHeThongConst.TSLa,
                             e => e.TaiSanId == null)
                         .WhereIf(input.BoPhanId != null && input.BoPhanId.Count > 0, e => input.BoPhanId.Contains(e.ToChuc))
                         .WhereIf(input.StartDate != null && input.EndDate != null, item => item.Ngay >= input.StartDate && item.Ngay <= input.EndDate)
                         from taiSan in this.taiSanRepository.GetAll().Where(e => e.Id == lichSu.TaiSanId)
                          .DefaultIfEmpty()
                         from toChuc in this.toChucRepository.GetAll().Where(e => e.Id == lichSu.ToChuc)
                         select new LichSuRaVaoForViewDto
                         {
                             Id = lichSu.Id,
                             MaRFID = lichSu.RFID,
                             TenTaiSan = taiSan.TenTaiSan,
                             DonViSuDung = toChuc.TenToChuc,
                             NgayDiChuyen = (DateTime)lichSu.Ngay,
                             ChieuDiChuyen = lichSu.Chieu,
                             PhanLoai = lichSu.TaiSanId != null ? "Tài sản trong hệ thống" : "Tài sản ngoài hệ thống",
                         }).WhereIf(!string.IsNullOrEmpty(keyWord), e => e.MaRFID.Contains(keyWord)
                                     || e.TenTaiSan.Contains(keyWord));

            int totalCount = await query.CountAsync();
            var result = await query.OrderBy(input.Sorting ?? "NgayDiChuyen DESC").PageBy(input).ToListAsync();
            return new PagedResultDto<LichSuRaVaoForViewDto>(totalCount, result);
        }

        public async Task<PagedResultDto<LichSuRaVaoForViewDto>> GetAllTaiSanLa(InputLichSuRaVaoDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            string keyWord = GlobalFunction.RegexFormat(input.Keyword);
            input.StartDate = GlobalFunction.GetDateTime(input.StartDate);
            input.EndDate = GlobalFunction.GetDateTime(input.EndDate);

            var query = (from lichSu in this.lichSuRaVaoRespository.GetAll()
                         .Where(e => e.TaiSanId == null)
                         .WhereIf(!string.IsNullOrEmpty(input.ChieuDiChuyen), e => e.Chieu == input.ChieuDiChuyen)
                         .WhereIf(
                             input.PhanLoaiId != null && input.PhanLoaiId == (int)GlobalConst.PhanLoaiTaiSanTrongHeThongConst.TSTrongHeThong,
                             e => e.TaiSanId != null)
                         .WhereIf(
                             input.PhanLoaiId != null && input.PhanLoaiId == (int)GlobalConst.PhanLoaiTaiSanTrongHeThongConst.TSLa,
                             e => e.TaiSanId == null)
                         .WhereIf(input.BoPhanId != null && input.BoPhanId.Count > 0, e => input.BoPhanId.Contains(e.ToChuc))
                         .WhereIf(input.StartDate != null && input.EndDate != null, item => item.Ngay >= input.StartDate && item.Ngay <= input.EndDate)
                         from taiSan in this.taiSanRepository.GetAll().Where(e => e.Id == lichSu.TaiSanId)
                          .DefaultIfEmpty()
                         from toChuc in this.toChucRepository.GetAll().Where(e => e.Id == lichSu.ToChuc)
                         select new LichSuRaVaoForViewDto
                         {
                             Id = lichSu.Id,
                             MaRFID = lichSu.RFID,
                             TenTaiSan = taiSan.TenTaiSan,
                             DonViSuDung = toChuc.TenToChuc,
                             NgayDiChuyen = (DateTime)lichSu.Ngay,
                             ChieuDiChuyen = lichSu.Chieu,
                             PhanLoai = lichSu.TaiSanId != null ? "Tài sản trong hệ thống" : "Tài sản ngoài hệ thống",
                         }).WhereIf(!string.IsNullOrEmpty(keyWord), e => e.MaRFID.Contains(keyWord)
                                     || e.TenTaiSan.Contains(keyWord));

            int totalCount = await query.CountAsync();
            var result = await query.OrderBy(input.Sorting ?? "NgayDiChuyen DESC").PageBy(input).ToListAsync();
            return new PagedResultDto<LichSuRaVaoForViewDto>(totalCount, result);
        }

        public async Task<List<LookupTableDto>> GetAllNhaCC()
        {
            var filter = await this.nhaCungCapRepository.GetAll().Select(e => new LookupTableDto()
            {
                Id = e.Id,
                DisplayName = e.TenNhaCungCap,
            }).ToListAsync();
            return filter;
        }

        public async Task<FileDto> ExportTaiSanLaToExcel(InputLichSuRaVaoDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            // L?y danh s?ch c?n xu?t excel
            var list = await this.GetAllTaiSanLa(input);
            using var package = new ExcelPackage();

            // Add sheet
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

            var namedStyle = package.Workbook.Styles.CreateNamedStyle("HyperLink");
            namedStyle.Style.Font.UnderLine = true;
            namedStyle.Style.Font.Color.SetColor(Color.Blue);

            // set header
            worksheet.Cells[1, 1].Value = "Mã RFID";
            worksheet.Cells[1, 2].Value = "Ngày di chuyển";

            // B?i ??m header
            using (ExcelRange r = worksheet.Cells[1, 1, 1, 2])
            {
                using var f = new Font("Calibri", 12, FontStyle.Bold);
                r.Style.Font.SetFromFont(f);
                r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            }

            // Gan gia tri
            var rowNumber = 2;
            list.Items.ToList().ForEach(item =>
            {
                worksheet.Cells[rowNumber, 1].Value = item.MaRFID;
                worksheet.Cells[rowNumber, 2].Value = GlobalFunction.GetDateTimeStringFromDate(item.NgayDiChuyen);
                rowNumber++;
            });

            // Cho c?c ? r?ng theo d? li?u
            worksheet.Cells.AutoFitColumns(0);

            worksheet.PrinterSettings.FitToHeight = 1;

            // T?n file
            var fileName = string.Join(".", new string[] { "Danh sách theo dõi kết nối thiết bị", "xlsx" });

            // L?u file v?o server
            using (var stream = new MemoryStream())
            {
                package.SaveAs(stream);
            }

            var file = new FileDto(fileName, MimeTypeNames.ApplicationVndOpenxmlformatsOfficedocumentSpreadsheetmlSheet);
            var filePath = Path.Combine(this.appFolders.TempFileDownloadFolder, file.FileToken);
            package.SaveAs(new FileInfo(filePath));
            return file;
        }

        public async Task<FileDto> ExportLichSuRaVaoToExcel(InputLichSuRaVaoDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            // L?y danh s?ch c?n xu?t excel
            var list = await this.GetAll(input);
            using var package = new ExcelPackage();

            // Add sheet
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

            var namedStyle = package.Workbook.Styles.CreateNamedStyle("HyperLink");
            namedStyle.Style.Font.UnderLine = true;
            namedStyle.Style.Font.Color.SetColor(Color.Blue);

            // set header
            worksheet.Cells[1, 1].Value = "Mã RFID";
            worksheet.Cells[1, 2].Value = "Tên tài sản";
            worksheet.Cells[1, 3].Value = "Đơn vị quản lý";
            worksheet.Cells[1, 4].Value = "Ngày di chuyển";
            worksheet.Cells[1, 5].Value = "Chiều di chuyển";
            worksheet.Cells[1, 6].Value = "Phân loại";

            // B?i ??m header
            using (ExcelRange r = worksheet.Cells[1, 1, 1, 6])
            {
                using var f = new Font("Calibri", 12, FontStyle.Bold);
                r.Style.Font.SetFromFont(f);
                r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            }

            // Gan gia tri
            var rowNumber = 2;
            list.Items.ToList().ForEach(item =>
            {
                worksheet.Cells[rowNumber, 1].Value = item.MaRFID;
                worksheet.Cells[rowNumber, 2].Value = item.TenTaiSan;
                worksheet.Cells[rowNumber, 3].Value = item.DonViSuDung;
                worksheet.Cells[rowNumber, 4].Value = GlobalFunction.GetDateTimeStringFromDate(item.NgayDiChuyen);
                worksheet.Cells[rowNumber, 5].Value = item.ChieuDiChuyen;
                worksheet.Cells[rowNumber, 6].Value = item.PhanLoai;
                rowNumber++;
            });

            // Cho c?c ? r?ng theo d? li?u
            worksheet.Cells.AutoFitColumns(0);

            worksheet.PrinterSettings.FitToHeight = 1;

            // T?n file
            var fileName = string.Join(".", new string[] { "Danh sách giám sát tài sản", "xlsx" });

            // L?u file v?o server
            using (var stream = new MemoryStream())
            {
                package.SaveAs(stream);
            }

            var file = new FileDto(fileName, MimeTypeNames.ApplicationVndOpenxmlformatsOfficedocumentSpreadsheetmlSheet);
            var filePath = Path.Combine(this.appFolders.TempFileDownloadFolder, file.FileToken);
            package.SaveAs(new FileInfo(filePath));
            return file;
        }

        public void Update(UpdateInputDto input)
        {
            // check null input
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }

            var update = this.lichSuRaVaoRespository.GetAll().WhereIf(!string.IsNullOrEmpty(input.RFID), item => item.RFID == input.RFID).ToList();
            if (update.Count > 0)
            {
                try
                {
                    update.ForEach(x => x.TaiSanId = input.TaiSanId);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
