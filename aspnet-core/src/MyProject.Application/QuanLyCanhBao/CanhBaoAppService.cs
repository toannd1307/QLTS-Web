namespace MyProject.QuanLyCanhBao
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Text;
    using System.Threading.Tasks;
    using System.Timers;
    using Abp.Application.Services.Dto;
    using Abp.Authorization;
    using Abp.Domain.Repositories;
    using Abp.Linq.Extensions;
    using Abp.UI;
    using DbEntities;
    using Microsoft.EntityFrameworkCore;
    using MyProject.Authorization;
    using MyProject.Authorization.Users;
    using MyProject.Data;
    using MyProject.Data.Excel.Dtos;
    using MyProject.Global;
    using MyProject.Global.Dtos;
    using MyProject.Net.MimeTypes;
    using MyProject.QuanLyCanhBao.Dtos;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class CanhBaoAppService : MyProjectAppServiceBase, ICanhBaoAppService
    {
        private readonly IRepository<CanhBao> canhBaoRepository;
        private readonly IRepository<ToChuc> toChucRepository;
        private readonly IRepository<User, long> userRepository;
        private readonly IAppFolders appFolders;
        private readonly IRepository<TaiSan> taiSanRepository;
        private readonly IRepository<ViTriDiaLy> viTriRepository;
        private readonly IRepository<LoaiTaiSan> loaiTaiSanRepository;

        public CanhBaoAppService(
            IRepository<CanhBao> canhBaoRepository,
            IAppFolders appFolders,
            IRepository<ToChuc> toChucRepository,
            IRepository<User, long> userRepository,
            IRepository<TaiSan> taiSanRepository,
            IRepository<ViTriDiaLy> viTriRepository,
            IRepository<LoaiTaiSan> loaiTaiSanRepository)
        {
            this.canhBaoRepository = canhBaoRepository;
            this.appFolders = appFolders;
            this.toChucRepository = toChucRepository;
            this.userRepository = userRepository;
            this.taiSanRepository = taiSanRepository;
            this.viTriRepository = viTriRepository;
            this.loaiTaiSanRepository = loaiTaiSanRepository;
        }

        public async Task<PagedResultDto<CanhBaoForViewDto>> GetAllAsync(CanhBaoInputDto input)
        {
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }

            var pbIdUser = this.userRepository.GetAll().Where(w => w.Id == this.AbpSession.UserId).Select(s => s.ToChucId).FirstOrDefault();
            var listPbDangNhap = new List<int>();
            listPbDangNhap = GlobalFunction.GetAllToChucIdTheoNguoiDungListAsync(this.toChucRepository, (int)pbIdUser).Result;

            input.NoiDung = GlobalFunction.RegexFormat(input.NoiDung);
            input.ThoiGianFrom = GlobalFunction.GetDateTime(input.ThoiGianFrom);
            input.ThoiGianTo = GlobalFunction.GetDateTime(input.ThoiGianTo);
            var query = from canhBao in this.canhBaoRepository.GetAll().Where(w => (w.NoiDung.Contains(input.NoiDung) || input.NoiDung == null) && (input.ToChucId != null ? input.ToChucId.Contains((int)w.PhongBanNhanId) : listPbDangNhap.Contains((int)w.PhongBanNhanId))
                        && (w.PhanLoai == input.HoatDong || input.HoatDong == null) && (w.TaiKhoanId == input.TaiKhoanId || input.TaiKhoanId == null)
                        && (input.ThoiGianFrom.HasValue ? w.CreationTime.Date >= input.ThoiGianFrom.Value.Date : input.ThoiGianFrom == null)
                        && (input.ThoiGianTo.HasValue ? w.CreationTime.Date <= input.ThoiGianTo.Value.Date : input.ThoiGianTo == null)
                        && w.NguoiNhanId == this.AbpSession.UserId)
                        from tochuc in this.toChucRepository.GetAll().Where(w => w.Id == canhBao.PhongBanNhanId)
                        from phongBanCha in this.toChucRepository.GetAll().Where(w => w.Id == tochuc.TrucThuocToChucId).DefaultIfEmpty()
                        select new CanhBaoForViewDto()
                        {
                            Id = canhBao.Id,
                            TaiKhoanId = canhBao.TaiKhoanId,
                            ToChucId = tochuc.Id,
                            NoiDung = canhBao.NoiDung,
                            ToChuc = (tochuc.TrucThuocToChucId > 1 ? (phongBanCha.MaToChuc + " - ") : string.Empty) + tochuc.TenToChuc,
                            ThoiGian = canhBao.CreationTime.ToString("dd/MM/yyyy - h:mm"),
                            Date = canhBao.CreationTime,
                        };

            int totalCount = await query.CountAsync();
            var output = await query.OrderBy(input.Sorting ?? "Date DESC")
                .PageBy(input)
                .ToListAsync();
            return new PagedResultDto<CanhBaoForViewDto>(totalCount, output);
        }

        public async Task<PagedResultDto<ThongBaoOutput>> GetAllThongBao(ThongBaoInput input)
        {
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }

            var pbIdUser = this.userRepository.GetAll().Where(w => w.Id == this.AbpSession.UserId).Select(s => s.ToChucId).FirstOrDefault();
            var listPbDangNhap = new List<int>();
            listPbDangNhap = GlobalFunction.GetAllToChucIdTheoNguoiDungListAsync(this.toChucRepository, (int)pbIdUser).Result;

            var query = from canhBao in this.canhBaoRepository.GetAll().Where(w => (w.TrangThai == false) && listPbDangNhap.Contains((int)w.PhongBanNhanId) && w.NguoiNhanId == this.AbpSession.UserId)
                        select new ThongBaoOutput()
                        {
                            NoiDung = canhBao.NoiDung,
                            ThoiGian = (DateTime.Now.Subtract(canhBao.CreationTime).Days == 0 && DateTime.Now.Subtract(canhBao.CreationTime).Hours == 0 && DateTime.Now.Subtract(canhBao.CreationTime).Minutes < 60) ? (DateTime.Now.Subtract(canhBao.CreationTime).Minutes == 0 ? "Vừa xong" : DateTime.Now.Subtract(canhBao.CreationTime).Minutes + " phút trước") : (DateTime.Now.Subtract(canhBao.CreationTime).Days == 0 && DateTime.Now.Subtract(canhBao.CreationTime).Hours > 0 ? DateTime.Now.Subtract(canhBao.CreationTime).Hours + " giờ trước" : canhBao.CreationTime.ToString("dd/MM/yyyy")),
                            Ngay = canhBao.CreationTime,
                        };

            int totalCount = await query.CountAsync();
            var output = await query.OrderBy(input.Sorting ?? "Ngay DESC")
                .PageBy(input)
                .ToListAsync();
            return new PagedResultDto<ThongBaoOutput>(totalCount, output);
        }

        public async Task<List<LookupTableDto>> GetAllNguoiDung()
        {
            var pbIdUser = this.userRepository.GetAll().Where(w => w.Id == this.AbpSession.UserId).Select(s => s.ToChucId).FirstOrDefault();
            var listPbDangNhap = new List<int>();
            listPbDangNhap = GlobalFunction.GetAllToChucIdTheoNguoiDungListAsync(this.toChucRepository, (int)pbIdUser).Result;

            var query = (from canhBao in this.canhBaoRepository.GetAll().Where(w => (w.TrangThai == false) && w.TaiKhoanId != null && listPbDangNhap.Contains((int)w.PhongBanNhanId) && w.NguoiNhanId == this.AbpSession.UserId)
                         from us in this.userRepository.GetAll().Where(w => w.Id == canhBao.TaiKhoanId)
                         select new LookupTableDto
                         {
                             Id = (int)us.Id,
                             DisplayName = us.UserName,
                         }).Distinct().ToList();
            return await Task.FromResult(query);
        }

        public async Task<FileDto> ExportToExcel(CanhBaoInputDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            // Lấy danh sách cần xuất excel
            var list = await this.GetAllAsync(input);
            using var package = new ExcelPackage();

            // Add sheet
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

            var namedStyle = package.Workbook.Styles.CreateNamedStyle("HyperLink");
            namedStyle.Style.Font.UnderLine = true;
            namedStyle.Style.Font.Color.SetColor(Color.Blue);

            // set header
            worksheet.Cells[1, 1].Value = "STT";
            worksheet.Cells[1, 2].Value = "Nội dung";
            worksheet.Cells[1, 3].Value = "Đơn vị";
            worksheet.Cells[1, 4].Value = "Thời gian";

            // Bôi đậm header
            using (ExcelRange r = worksheet.Cells[1, 1, 1, 4])
            {
                using var f = new Font("Calibri", 12, FontStyle.Bold);
                r.Style.Font.SetFromFont(f);
                r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            }

            // Gan gia tri
            var rowNumber = 2;
            list.Items.ToList().ForEach(item =>
            {
                worksheet.Cells[rowNumber, 1].Value = rowNumber - 1;
                worksheet.Cells[rowNumber, 2].Value = item.NoiDung;
                worksheet.Cells[rowNumber, 3].Value = item.ToChuc;
                worksheet.Cells[rowNumber, 4].Value = item.ThoiGian;
                rowNumber++;
            });

            // Cho các ô rộng theo dữ liệu
            worksheet.Cells.AutoFitColumns(0);

            worksheet.PrinterSettings.FitToHeight = 1;

            // Tên file
            var fileName = string.Join(".", new string[] { "Danh sách cảnh báo", "xlsx" });

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

        public async Task TaoCanhBaoAsync(InputFromServiceDto input)
        {
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }

            var listUser = this.userRepository.GetAllList();
            var check = false;// Dùng để đánh dấu bản ghi đầu tiên cho chức năng được thông báo -> báo cáo cảnh báo
            foreach (var item in listUser)
            {
                // check user có quyền nhận thông báo của phòng ban nhận thông báo
                var listPb = GlobalFunction.GetAllToChucIdTheoNguoiDungListAsync(this.toChucRepository, (int)item.ToChucId).Result;
                if (await this.PermissionChecker.IsGrantedAsync(item.ToUserIdentifier(), PermissionNames.Pages_QuanLyCanhBao) && listPb.Contains((int)input.PhongBanNhanId))
                {
                    var create = new CanhBao();
                    create.TaiKhoanId = input.TaiKhoanId;
                    create.PhongBanNhanId = input.PhongBanNhanId;
                    create.NguoiNhanId = (int)item.Id;
                    create.NoiDung = input.NoiDung;
                    create.PhanLoai = input.LoaiCanhBao;
                    create.TrangThai = false;
                    create.LichSuRaVaoDaGuiId = input.IdLichSu;
                    if (check == false)
                    {
                        create.IsCheckBaoCao = true;
                        check = true;
                    }

                    await this.canhBaoRepository.InsertAsync(create);
                }
            }
        }
    }
}
